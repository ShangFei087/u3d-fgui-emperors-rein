#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
从 Unity / slot_game_record_export 导出的游戏记录 CSV 生成同结构汇总 xlsx/csv。

用法:
  python slot_game_record_summary_report.py --input xxx_record.csv
  # 默认与源文件同目录：xxx_record.xlsx、xxx_record_汇总.csv（不覆盖源 csv）
  python slot_game_record_summary_report.py --input xxx_record.csv --out-xlsx a.xlsx --out-csv b.csv
  python slot_game_record_summary_report.py --input xxx_record.csv --block2-name FIREBIRD

依赖: pip install openpyxl（仅写 xlsx 时需要）
"""

from __future__ import annotations

import argparse
import csv
import sys
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional, Sequence, Tuple

# 普通（仅「赢」局）：左闭右开，最后一档 [20, +∞)
MULT_BUCKETS_NORMAL: Sequence[Tuple[str, float, Optional[float]]] = (
    ("1倍以下", 0.0, 1.0),
    ("1-2倍", 1.0, 2.0),
    ("2-5倍", 2.0, 5.0),
    ("5-10倍", 5.0, 10.0),
    ("10-20倍", 10.0, 20.0),
    ("20倍以上", 20.0, None),
)

# 大奖：200～600 倍，分界 200/300/400/500/600
MULT_BUCKETS_BIGWIN: Sequence[Tuple[str, float, Optional[float]]] = (
    ("200倍以下", 0.0, 200.0),
    ("200-300倍", 200.0, 300.0),
    ("300-400倍", 300.0, 400.0),
    ("400-500倍", 400.0, 500.0),
    ("500-600倍", 500.0, 600.0),
    ("600倍以上", 600.0, None),
)

# 免费：60～150 倍，分界 60/80/100/120/150
MULT_BUCKETS_FREE: Sequence[Tuple[str, float, Optional[float]]] = (
    ("60倍以下", 0.0, 60.0),
    ("60-80倍", 60.0, 80.0),
    ("80-100倍", 80.0, 100.0),
    ("100-120倍", 100.0, 120.0),
    ("120-150倍", 120.0, 150.0),
    ("150倍以上", 150.0, None),
)

# 兼容旧名
MULT_BUCKETS = MULT_BUCKETS_NORMAL


def _f(s: str) -> float:
    s = (s or "").strip()
    if not s:
        return 0.0
    try:
        return float(s)
    except ValueError:
        return 0.0


def _bucket_for(mult: float, defs: Sequence[Tuple[str, float, Optional[float]]]) -> str:
    for label, lo, hi in defs:
        if hi is None:
            if mult >= lo:
                return label
        else:
            if lo <= mult < hi:
                return label
    return defs[0][0]


def _bucket(mult: float) -> str:
    """默认：普通分档（兼容旧调用）。"""
    return _bucket_for(mult, MULT_BUCKETS_NORMAL)


# jackpot_type 1/2/3 → Major / Minor / Mini
JACKPOT_TYPE_ROWS: Sequence[Tuple[str, str]] = (
    ("1", "Major"),
    ("2", "Minor"),
    ("3", "Mini"),
)

@dataclass
class Row:
    col_a: str
    bet: float
    payout: float  # 「总得分」列；旧版 CSV 无该列时用 WIN+彩金得分
    jackpot_type: str = ""  # 1=Major，2=Minor，3=Mini

    @property
    def total_win(self) -> float:
        return self.payout

    @property
    def mult(self) -> float:
        if self.bet <= 0:
            return 0.0
        return self.payout / self.bet


def _normalize_header_cell(s: str) -> str:
    """去掉首尾空白与 BOM（Unity/部分编辑器会写出双 BOM，首列会变成 \\ufeff大奖统计）。"""
    return (s or "").strip().strip("\ufeff\u200b")


def _read_rows(path: Path) -> Tuple[List[str], List[Row]]:
    raw = path.read_bytes()
    # 去掉文件头连续 UTF-8 BOM，避免双 BOM 残留进首列表头
    while raw.startswith(b"\xef\xbb\xbf"):
        raw = raw[3:]
    text = raw.decode("utf-8", errors="replace")
    lines = text.splitlines()
    if not lines:
        return [], []
    reader = csv.reader(lines)
    header = [_normalize_header_cell(x) for x in next(reader)]
    idx = {name: i for i, name in enumerate(header) if name}

    def col(name: str, alt: str = "") -> int:
        if name in idx:
            return idx[name]
        if alt and alt in idx:
            return idx[alt]
        raise KeyError(f"列未找到: {name} (可选 {alt})")

    i_a = col("大奖统计")
    i_bet = col("下注")
    i_jp_type = idx.get("彩金类型", -1)
    try:
        i_payout = col("总得分")
    except KeyError:
        i_payout = -1
    i_win_legacy = -1
    i_jp_legacy = -1
    if i_payout < 0:
        i_win_legacy = col("WIN", "基础游戏得分")
        i_jp_legacy = col("彩金得分")

    rows: List[Row] = []
    for parts in reader:
        if not parts or all(not (c or "").strip() for c in parts):
            continue
        def get(i: int) -> str:
            return parts[i] if i < len(parts) else ""
        a = get(i_a).strip()
        # 若 CSV 在数据行后附加汇总附录，首列标记行以下不再作为游戏行解析
        if a == "#SLOT_GAME_RECORD_SUMMARY_START":
            break
        if i_payout >= 0:
            payout = _f(get(i_payout))
        else:
            payout = _f(get(i_win_legacy)) + _f(get(i_jp_legacy))
        rows.append(
            Row(
                col_a=a,
                bet=_f(get(i_bet)),
                payout=payout,
                jackpot_type=(get(i_jp_type).strip() if i_jp_type >= 0 else ""),
            )
        )
    return header, rows


def _detail_row(
    type_name: object,
    bet: float,
    count: object,
    win: float,
    avg: float,
    prob: float,
    rtp: float,
) -> List[object]:
    """明细一行：类型、总玩、局、赢分、平均（倍）、出现概率、RTP。"""
    return [
        type_name,
        round(bet, 4),
        count,
        round(win, 4),
        round(avg, 4),
        round(prob, 6),
        round(rtp, 6),
    ]


def _agg_bucketed(
    subset: List[Row],
    total_rounds: int,
    total_bet: float,
    bucket_defs: Sequence[Tuple[str, float, Optional[float]]],
) -> List[Dict[str, object]]:
    """返回若干行 dict: 类型, 局, 赢分, 平均倍, 出现概率, RTP"""
    out: List[Dict[str, object]] = []
    by_bucket: Dict[str, List[Row]] = defaultdict(list)
    for r in subset:
        by_bucket[_bucket_for(r.mult, bucket_defs)].append(r)

    for label, _, _ in bucket_defs:
        rs = by_bucket.get(label) or []
        n = len(rs)
        sw = sum(x.total_win for x in rs)
        sb = sum(x.bet for x in rs)
        avg = sw / sb if sb else 0.0
        prob = n / total_rounds if total_rounds else 0.0
        rtp = sw / total_bet if total_bet > 0 else 0.0
        out.append(
            {
                "类型": label,
                "局": n,
                "赢分": sw,
                "总玩": sb,
                "押分": sb,
                "平均倍": avg,
                "出现概率": prob,
                "RTP": rtp,
            }
        )

    n_all = len(subset)
    sw_all = sum(x.total_win for x in subset)
    sb_all = sum(x.bet for x in subset)
    avg_all = sw_all / sb_all if sb_all else 0.0
    prob_all = n_all / total_rounds if total_rounds else 0.0
    rtp_all = sw_all / total_bet if total_bet > 0 else 0.0
    out.append(
        {
            "类型": "小计",
            "局": n_all,
            "赢分": sw_all,
            "总玩": sb_all,
            "押分": sb_all,
            "平均倍": avg_all,
            "出现概率": prob_all,
            "RTP": rtp_all,
        }
    )
    return out


def _section_block(
    title: str,
    subset: List[Row],
    total_rounds: int,
    total_bet: float,
    use_sub_buckets: bool,
    bucket_defs: Optional[Sequence[Tuple[str, float, Optional[float]]]] = None,
) -> Tuple[List[List[object]], Dict[str, float]]:
    """返回 (块行, 小计 dict：局/赢分/平均倍/出现概率/RTP)。"""
    defs = bucket_defs if bucket_defs is not None else MULT_BUCKETS_NORMAL
    lines: List[List[object]] = []
    lines.append([title, "", "", "", "", "", ""])
    empty_sub = {"局": 0, "赢分": 0.0, "总玩": 0.0, "押分": 0.0, "平均倍": 0.0, "出现概率": 0.0, "RTP": 0.0}
    if not subset:
        if use_sub_buckets:
            agg = _agg_bucketed([], total_rounds, total_bet, defs)
            sub = empty_sub
            for d in agg:
                if d["类型"] == "小计":
                    sub = d
                lines.append(
                    _detail_row(
                        d["类型"],
                        float(d["总玩"]),
                        d["局"],
                        float(d["赢分"]),
                        float(d["平均倍"]),
                        float(d["出现概率"]),
                        float(d["RTP"]),
                    )
                )
        else:
            lines.append(_detail_row("小计", 0.0, 0, 0.0, 0.0, 0.0, 0.0))
            sub = empty_sub
        return lines, sub
    if use_sub_buckets:
        agg = _agg_bucketed(subset, total_rounds, total_bet, defs)
        sub = empty_sub
        for d in agg:
            if d["类型"] == "小计":
                sub = d
            lines.append(
                _detail_row(
                    d["类型"],
                    float(d["总玩"]),
                    d["局"],
                    float(d["赢分"]),
                    float(d["平均倍"]),
                    float(d["出现概率"]),
                    float(d["RTP"]),
                )
            )
        return lines, sub
    n = len(subset)
    sw = sum(x.total_win for x in subset)
    sb = sum(x.bet for x in subset)
    avg = sw / sb if sb else 0.0
    prob = n / total_rounds if total_rounds else 0.0
    rtp = sw / total_bet if total_bet > 0 else 0.0
    sub = {"局": n, "赢分": sw, "总玩": sb, "押分": sb, "平均倍": avg, "出现概率": prob, "RTP": rtp}
    lines.append(_detail_row("小计", sb, n, sw, avg, prob, rtp))
    return lines, sub


def _jackpot_type_section(
    jp_rows: List[Row],
    total_rounds: int,
    total_bet: float,
) -> Tuple[List[List[object]], Dict[str, float]]:
    """彩金块：Major/Minor/Mini + 小计。"""
    lines: List[List[object]] = [["彩金", "", "", "", "", "", ""]]
    for code, label in JACKPOT_TYPE_ROWS:
        typed = [r for r in jp_rows if (r.jackpot_type or "").strip() == code]
        n = len(typed)
        sw = sum(x.total_win for x in typed)
        sb = sum(x.bet for x in typed)
        avg = sw / sb if sb else 0.0
        prob = n / total_rounds if total_rounds else 0.0
        rtp = sw / total_bet if total_bet > 0 else 0.0
        lines.append(_detail_row(label, sb, n, sw, avg, prob, rtp))

    n = len(jp_rows)
    sw = sum(x.total_win for x in jp_rows)
    sb = sum(x.bet for x in jp_rows)
    avg = sw / sb if sb else 0.0
    prob = n / total_rounds if total_rounds else 0.0
    rtp = sw / total_bet if total_bet > 0 else 0.0
    sub = {"局": n, "赢分": sw, "总玩": sb, "押分": sb, "平均倍": avg, "出现概率": prob, "RTP": rtp}
    lines.append(_detail_row("小计", sb, n, sw, avg, prob, rtp))
    return lines, sub


def build_report(
    rows: List[Row],
    block2_name: str = "大奖",
    map_block2_from: str = "大奖",
    jp_section_use_buckets: bool = False,
) -> Tuple[List[List[object]], Dict[str, float]]:
    """返回 (表格行, 顶部统计 dict)"""
    total_rounds = len(rows)
    total_bet = sum(r.bet for r in rows)
    total_win_all = sum(r.total_win for r in rows)
    lose_count = sum(1 for r in rows if r.col_a == "输")
    lose_prob = lose_count / total_rounds if total_rounds else 0.0
    total_rtp = total_win_all / total_bet if total_bet > 0 else 0.0

    def subset(tag: str) -> List[Row]:
        return [r for r in rows if r.col_a == tag]

    normal = [r for r in rows if r.col_a == "赢"]
    lose_rows = subset("输")
    block2 = [r for r in rows if r.col_a == map_block2_from]
    jp_rows = subset("彩金")
    free_rows = subset("免费")

    big_rtp = sum(r.total_win for r in block2) / total_bet if total_bet > 0 else 0.0
    normal_rtp = sum(r.total_win for r in normal) / total_bet if total_bet > 0 else 0.0
    free_rtp = sum(r.total_win for r in free_rows) / total_bet if total_bet > 0 else 0.0
    jp_rtp = sum(r.total_win for r in jp_rows) / total_bet if total_bet > 0 else 0.0

    top_stats = {
        "总局": float(total_rounds),
        "输局": float(lose_count),
        "输局概率": lose_prob,
        "总玩分": total_bet,
        "总得分": total_win_all,
        "合计RTP": total_rtp,
        "大奖RTP": big_rtp,
        "普通游戏RTP": normal_rtp,
        "免费RTP": free_rtp,
        "彩金游戏RTP": jp_rtp,
    }

    header_detail = ["类型", "总玩", "局", "赢分", "平均（倍）", "出现概率", "RTP返还率"]
    grid: List[List[object]] = []
    grid.append(
        [
            "统计项",
            "总局",
            "总玩分",
            "总得分",
            "合计RTP",
            "大奖RTP",
            "普通游戏RTP",
            "免费RTP",
            "彩金游戏RTP",
        ]
    )
    grid.append(
        [
            "数值",
            total_rounds,
            round(total_bet, 4),
            round(total_win_all, 4),
            round(total_rtp, 6),
            round(big_rtp, 6),
            round(normal_rtp, 6),
            round(free_rtp, 6),
            round(jp_rtp, 6),
        ]
    )
    grid.append([])
    grid.append(header_detail)

    section_subtotals: List[Dict[str, float]] = []

    def _append_section(*args, **kwargs) -> None:
        block, sub = _section_block(*args, **kwargs)
        grid.extend(block)
        section_subtotals.append(sub)

    _append_section("普通", normal, total_rounds, total_bet, True, MULT_BUCKETS_NORMAL)
    grid.append([])
    _append_section("输", lose_rows, total_rounds, total_bet, False)
    grid.append([])
    _append_section(block2_name, block2, total_rounds, total_bet, True, MULT_BUCKETS_BIGWIN)
    grid.append([])
    jp_block, jp_sub = _jackpot_type_section(jp_rows, total_rounds, total_bet)
    grid.extend(jp_block)
    section_subtotals.append(jp_sub)
    grid.append([])
    _append_section("免费", free_rows, total_rounds, total_bet, True, MULT_BUCKETS_FREE)

    grid.append([])
    n_tot = sum(int(s["局"]) for s in section_subtotals)
    sw_tot = sum(float(s["赢分"]) for s in section_subtotals)
    sb_tot = sum(float(s.get("总玩", s.get("押分", 0))) for s in section_subtotals)
    avg_tot = sw_tot / sb_tot if sb_tot else 0.0
    prob_tot = sum(float(s["出现概率"]) for s in section_subtotals)
    rtp_tot = sum(float(s["RTP"]) for s in section_subtotals)
    grid.append(_detail_row("合计", sb_tot, n_tot, sw_tot, avg_tot, prob_tot, rtp_tot))

    return grid, top_stats


def write_csv(path: Path, grid: List[List[object]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f)
        for row in grid:
            w.writerow(row)


def write_xlsx(path: Path, grid: List[List[object]]) -> None:
    try:
        from openpyxl import Workbook
    except ImportError as e:
        raise SystemExit("写 xlsx 需要 openpyxl: pip install openpyxl") from e
    path.parent.mkdir(parents=True, exist_ok=True)
    wb = Workbook()
    ws = wb.active
    ws.title = "汇总"
    for r, row in enumerate(grid, start=1):
        for c, val in enumerate(row, start=1):
            ws.cell(row=r, column=c, value=val)
    wb.save(path)


def main() -> None:
    ap = argparse.ArgumentParser(description="游戏记录 CSV → 汇总表 (xlsx/csv)")
    ap.add_argument("--input", "-i", type=Path, required=True, help="游戏记录 CSV 路径（如 *_record.csv）")
    ap.add_argument(
        "--out-xlsx",
        type=Path,
        default=None,
        help="输出 xlsx（默认: 与输入同路径、同主文件名，仅扩展名为 .xlsx）",
    )
    ap.add_argument(
        "--out-csv",
        type=Path,
        default=None,
        help="输出汇总 csv（默认: 同目录 <主文件名>_汇总.csv，避免与源 CSV 重名）",
    )
    ap.add_argument(
        "--block2-name",
        default="大奖",
        help="第二块标题（截图里常为 FIREBIRD；3997 无该列时用「大奖」）",
    )
    ap.add_argument(
        "--block2-from-col-a",
        default="大奖",
        help="第二块数据来源：A 列等于该文案的行（默认 大奖）",
    )
    ap.add_argument(
        "--jp-buckets",
        action="store_true",
        help="彩金块也按倍数分档（默认仅一行小计）",
    )
    args = ap.parse_args()

    if not args.input.is_file():
        print(f"文件不存在: {args.input}", file=sys.stderr)
        sys.exit(1)

    _, rows = _read_rows(args.input)
    if not rows:
        print("无数据行", file=sys.stderr)
        sys.exit(1)

    # 彩金块：默认单档小计；需要与 FIREBIRD 完全同款多档时可开 --jp-buckets
    def section_jp_buckets() -> bool:
        return bool(args.jp_buckets)

    grid, _ = build_report(
        rows,
        block2_name=args.block2_name,
        map_block2_from=args.block2_from_col_a,
        jp_section_use_buckets=section_jp_buckets(),
    )

    inp = args.input.resolve()
    out_xlsx = args.out_xlsx or inp.with_suffix(".xlsx")
    out_csv = args.out_csv or inp.with_name(f"{inp.stem}_汇总.csv")

    write_csv(out_csv, grid)
    print(f"已写 CSV: {out_csv}")

    try:
        write_xlsx(out_xlsx, grid)
        print(f"已写 XLSX: {out_xlsx}")
    except SystemExit as e:
        print(str(e), file=sys.stderr)


if __name__ == "__main__":
    main()
