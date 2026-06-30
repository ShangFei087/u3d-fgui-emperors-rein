package com.lftlive.com.pag;

import android.graphics.Rect;
import android.util.DisplayMetrics;
import android.view.Gravity;
import android.view.ViewGroup;
import android.widget.FrameLayout;

/**
 * 根据 positionType / place 计算 PAG  overlay 布局。
 */
final class PagLayoutHelper {

    static final class LayoutSpec {
        int gravity = Gravity.CENTER;
        int width = ViewGroup.LayoutParams.MATCH_PARENT;
        int height = ViewGroup.LayoutParams.MATCH_PARENT;
        int leftMargin;
        int topMargin;
        int rightMargin;
        int bottomMargin;
    }

    private PagLayoutHelper() {
    }

    static LayoutSpec resolve(DisplayMetrics metrics, String positionType, String place,
                              float rightAdaptiveW, float rightAdaptiveH) {
        LayoutSpec spec = new LayoutSpec();
        int screenW = metrics.widthPixels;
        int screenH = metrics.heightPixels;

        String type = positionType == null ? "" : positionType.trim().toLowerCase();
        String layoutPlace = place == null ? "" : place.trim().toLowerCase();

        if ("full".equals(type) || "fullscreen".equals(layoutPlace)) {
            return spec;
        }

        if ("right".equals(type)) {
            float ratio = rightAdaptiveH > 0f ? (rightAdaptiveW / rightAdaptiveH) : 1f;
            int targetH = (int) (screenH * 0.85f);
            int targetW = (int) (targetH * ratio);
            if (targetW > screenW * 0.55f) {
                targetW = (int) (screenW * 0.55f);
                targetH = (int) (targetW / Math.max(ratio, 0.01f));
            }
            spec.gravity = Gravity.END | Gravity.CENTER_VERTICAL;
            spec.width = targetW;
            spec.height = targetH;
            spec.rightMargin = (int) (screenW * 0.02f);
            return spec;
        }

        if ("left".equals(type)) {
            spec.gravity = Gravity.START | Gravity.CENTER_VERTICAL;
            spec.width = (int) (screenW * 0.45f);
            spec.height = (int) (screenH * 0.45f);
            spec.leftMargin = (int) (screenW * 0.02f);
            return spec;
        }

        if ("top".equals(type)) {
            spec.gravity = Gravity.TOP | Gravity.CENTER_HORIZONTAL;
            spec.width = (int) (screenW * 0.8f);
            spec.height = (int) (screenH * 0.35f);
            spec.topMargin = (int) (screenH * 0.05f);
            return spec;
        }

        if ("bottom".equals(type)) {
            spec.gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
            spec.width = (int) (screenW * 0.8f);
            spec.height = (int) (screenH * 0.35f);
            spec.bottomMargin = (int) (screenH * 0.05f);
            return spec;
        }

        if ("turntable".equals(layoutPlace) || "turn_table".equals(layoutPlace)) {
            spec.gravity = Gravity.CENTER;
            spec.width = (int) (screenW * 0.72f);
            spec.height = (int) (screenH * 0.72f);
            return spec;
        }

        if ("jackpot".equals(layoutPlace)) {
            spec.gravity = Gravity.TOP | Gravity.CENTER_HORIZONTAL;
            spec.width = (int) (screenW * 0.9f);
            spec.height = (int) (screenH * 0.4f);
            spec.topMargin = (int) (screenH * 0.08f);
            return spec;
        }

        // center / default
        spec.gravity = Gravity.CENTER;
        spec.width = (int) (screenW * 0.75f);
        spec.height = (int) (screenH * 0.75f);
        return spec;
    }

    static void apply(FrameLayout.LayoutParams params, LayoutSpec spec) {
        params.gravity = spec.gravity;
        params.width = spec.width;
        params.height = spec.height;
        params.setMargins(spec.leftMargin, spec.topMargin, spec.rightMargin, spec.bottomMargin);
    }

    static Rect parseCustomRect(DisplayMetrics metrics, String extra) {
        if (extra == null || extra.isEmpty()) {
            return null;
        }
        String[] parts = extra.split(",");
        if (parts.length != 4) {
            return null;
        }
        try {
            float x = Float.parseFloat(parts[0].trim());
            float y = Float.parseFloat(parts[1].trim());
            float w = Float.parseFloat(parts[2].trim());
            float h = Float.parseFloat(parts[3].trim());
            int left = (int) (metrics.widthPixels * x);
            int top = (int) (metrics.heightPixels * y);
            int width = (int) (metrics.widthPixels * w);
            int height = (int) (metrics.heightPixels * h);
            return new Rect(left, top, left + width, top + height);
        } catch (NumberFormatException ignored) {
            return null;
        }
    }
}
