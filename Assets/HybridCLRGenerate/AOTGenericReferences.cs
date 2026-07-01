using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"Newtonsoft.Json.dll",
		"SelfAOT.dll",
		"System.Core.dll",
		"System.dll",
		"UnityEngine.AndroidJNIModule.dll",
		"UnityEngine.AssetBundleModule.dll",
		"UnityEngine.CoreModule.dll",
		"UnityEngine.JSONSerializeModule.dll",
		"mscorlib.dll",
		"spine-csharp.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// ScriptableObjectSingleton<object>
	// Spine.ExposedList.Enumerator<object>
	// Spine.ExposedList<object>
	// System.Action<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Action<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Action<Loom.DelayedQueueItem>
	// System.Action<Loom.NoDelayedQueueItem>
	// System.Action<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Action<System.DateTime>
	// System.Action<System.Guid>
	// System.Action<System.IntPtr>
	// System.Action<System.ValueTuple<int,object,double>>
	// System.Action<System.ValueTuple<int,object>>
	// System.Action<System.ValueTuple<object,int>>
	// System.Action<UnityEngine.Color32>
	// System.Action<UnityEngine.Vector2>
	// System.Action<byte,object>
	// System.Action<byte>
	// System.Action<float>
	// System.Action<int,System.IntPtr>
	// System.Action<int,int>
	// System.Action<int>
	// System.Action<long>
	// System.Action<object,object>
	// System.Action<object>
	// System.Action<ushort>
	// System.Collections.Concurrent.ConcurrentDictionary.<GetEnumerator>d__32<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary.DictionaryEnumerator<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Node<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Tables<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary<System.Guid,object>
	// System.Collections.Generic.ArraySortHelper<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.ArraySortHelper<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.ArraySortHelper<Loom.DelayedQueueItem>
	// System.Collections.Generic.ArraySortHelper<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ArraySortHelper<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.ArraySortHelper<System.DateTime>
	// System.Collections.Generic.ArraySortHelper<System.Guid>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<int,object>>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<object,int>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Color32>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2>
	// System.Collections.Generic.ArraySortHelper<byte>
	// System.Collections.Generic.ArraySortHelper<float>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<long>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.ArraySortHelper<ushort>
	// System.Collections.Generic.Comparer<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.Comparer<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.Comparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.Comparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.Comparer<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.Comparer<System.DateTime>
	// System.Collections.Generic.Comparer<System.Guid>
	// System.Collections.Generic.Comparer<System.Nullable<double>>
	// System.Collections.Generic.Comparer<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.Comparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<object,int>>
	// System.Collections.Generic.Comparer<UnityEngine.Color32>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<double>
	// System.Collections.Generic.Comparer<float>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<long>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Comparer<ushort>
	// System.Collections.Generic.Dictionary.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,ulong>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,System.IntPtr>
	// System.Collections.Generic.Dictionary.Enumerator<object,double>
	// System.Collections.Generic.Dictionary.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,long>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,float>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,ulong>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,System.IntPtr>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,double>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,long>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,ulong>
	// System.Collections.Generic.Dictionary.KeyCollection<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,System.IntPtr>
	// System.Collections.Generic.Dictionary.KeyCollection<object,double>
	// System.Collections.Generic.Dictionary.KeyCollection<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,long>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,float>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,int>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,ulong>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,System.IntPtr>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,double>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,long>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,ulong>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,System.IntPtr>
	// System.Collections.Generic.Dictionary.ValueCollection<object,double>
	// System.Collections.Generic.Dictionary.ValueCollection<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,long>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,float>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,int>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,object>
	// System.Collections.Generic.Dictionary<int,byte>
	// System.Collections.Generic.Dictionary<int,float>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<int,ulong>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary<object,System.IntPtr>
	// System.Collections.Generic.Dictionary<object,double>
	// System.Collections.Generic.Dictionary<object,float>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,long>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<ulong,float>
	// System.Collections.Generic.Dictionary<ulong,int>
	// System.Collections.Generic.Dictionary<ulong,object>
	// System.Collections.Generic.EqualityComparer<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.EqualityComparer<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.EqualityComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.EqualityComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.EqualityComparer<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.EqualityComparer<System.DateTime>
	// System.Collections.Generic.EqualityComparer<System.Guid>
	// System.Collections.Generic.EqualityComparer<System.IntPtr>
	// System.Collections.Generic.EqualityComparer<System.Nullable<double>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<object,int>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Color32>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<double>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<long>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<ulong>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<long>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<long>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<long>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.ICollection<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.ICollection<Loom.DelayedQueueItem>
	// System.Collections.Generic.ICollection<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ICollection<PagSegment>
	// System.Collections.Generic.ICollection<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,ulong>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,System.IntPtr>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,double>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ICollection<System.DateTime>
	// System.Collections.Generic.ICollection<System.Guid>
	// System.Collections.Generic.ICollection<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.ICollection<System.ValueTuple<int,object>>
	// System.Collections.Generic.ICollection<System.ValueTuple<object,int>>
	// System.Collections.Generic.ICollection<UnityEngine.Color32>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2>
	// System.Collections.Generic.ICollection<byte>
	// System.Collections.Generic.ICollection<float>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<long>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.ICollection<ushort>
	// System.Collections.Generic.IComparer<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.IComparer<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.IComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.IComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IComparer<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.IComparer<System.DateTime>
	// System.Collections.Generic.IComparer<System.Guid>
	// System.Collections.Generic.IComparer<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.IComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.IComparer<System.ValueTuple<object,int>>
	// System.Collections.Generic.IComparer<UnityEngine.Color32>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2>
	// System.Collections.Generic.IComparer<byte>
	// System.Collections.Generic.IComparer<float>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<long>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IComparer<ushort>
	// System.Collections.Generic.IDictionary<System.Guid,object>
	// System.Collections.Generic.IEnumerable<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.IEnumerable<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.IEnumerable<Loom.DelayedQueueItem>
	// System.Collections.Generic.IEnumerable<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IEnumerable<PagSegment>
	// System.Collections.Generic.IEnumerable<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Guid,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,ulong>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,System.IntPtr>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,double>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerable<System.DateTime>
	// System.Collections.Generic.IEnumerable<System.Guid>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<int,object>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<object,int>>
	// System.Collections.Generic.IEnumerable<UnityEngine.Color32>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerable<byte>
	// System.Collections.Generic.IEnumerable<float>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<long>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerable<ushort>
	// System.Collections.Generic.IEnumerator<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.IEnumerator<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.IEnumerator<Loom.DelayedQueueItem>
	// System.Collections.Generic.IEnumerator<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IEnumerator<PagSegment>
	// System.Collections.Generic.IEnumerator<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.Guid,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,ulong>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,System.IntPtr>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,double>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerator<System.DateTime>
	// System.Collections.Generic.IEnumerator<System.Guid>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<int,object>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<object,int>>
	// System.Collections.Generic.IEnumerator<UnityEngine.Color32>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerator<byte>
	// System.Collections.Generic.IEnumerator<float>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<long>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEnumerator<ushort>
	// System.Collections.Generic.IEqualityComparer<System.Guid>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<long>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<ulong>
	// System.Collections.Generic.IList<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.IList<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.IList<Loom.DelayedQueueItem>
	// System.Collections.Generic.IList<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IList<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<ulong,float>>
	// System.Collections.Generic.IList<System.DateTime>
	// System.Collections.Generic.IList<System.Guid>
	// System.Collections.Generic.IList<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.IList<System.ValueTuple<int,object>>
	// System.Collections.Generic.IList<System.ValueTuple<object,int>>
	// System.Collections.Generic.IList<UnityEngine.Color32>
	// System.Collections.Generic.IList<UnityEngine.Vector2>
	// System.Collections.Generic.IList<byte>
	// System.Collections.Generic.IList<float>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<long>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IList<ushort>
	// System.Collections.Generic.IReadOnlyCollection<PagSegment>
	// System.Collections.Generic.IReadOnlyCollection<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.IReadOnlyCollection<System.ValueTuple<int,object>>
	// System.Collections.Generic.IReadOnlyCollection<object>
	// System.Collections.Generic.IReadOnlyList<PagSegment>
	// System.Collections.Generic.IReadOnlyList<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.IReadOnlyList<System.ValueTuple<int,object>>
	// System.Collections.Generic.IReadOnlyList<object>
	// System.Collections.Generic.KeyValuePair<System.Guid,object>
	// System.Collections.Generic.KeyValuePair<int,byte>
	// System.Collections.Generic.KeyValuePair<int,float>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<int,ulong>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<object,System.IntPtr>
	// System.Collections.Generic.KeyValuePair<object,double>
	// System.Collections.Generic.KeyValuePair<object,float>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,long>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<ulong,float>
	// System.Collections.Generic.KeyValuePair<ulong,int>
	// System.Collections.Generic.KeyValuePair<ulong,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.List.Enumerator<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.List.Enumerator<Loom.DelayedQueueItem>
	// System.Collections.Generic.List.Enumerator<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.List.Enumerator<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.List.Enumerator<System.DateTime>
	// System.Collections.Generic.List.Enumerator<System.Guid>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<int,object>>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<object,int>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Color32>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2>
	// System.Collections.Generic.List.Enumerator<byte>
	// System.Collections.Generic.List.Enumerator<float>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List.Enumerator<ushort>
	// System.Collections.Generic.List.SynchronizedList<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.List.SynchronizedList<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.List.SynchronizedList<Loom.DelayedQueueItem>
	// System.Collections.Generic.List.SynchronizedList<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.List.SynchronizedList<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.List.SynchronizedList<System.DateTime>
	// System.Collections.Generic.List.SynchronizedList<System.Guid>
	// System.Collections.Generic.List.SynchronizedList<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.List.SynchronizedList<System.ValueTuple<int,object>>
	// System.Collections.Generic.List.SynchronizedList<System.ValueTuple<object,int>>
	// System.Collections.Generic.List.SynchronizedList<UnityEngine.Color32>
	// System.Collections.Generic.List.SynchronizedList<UnityEngine.Vector2>
	// System.Collections.Generic.List.SynchronizedList<byte>
	// System.Collections.Generic.List.SynchronizedList<float>
	// System.Collections.Generic.List.SynchronizedList<int>
	// System.Collections.Generic.List.SynchronizedList<long>
	// System.Collections.Generic.List.SynchronizedList<object>
	// System.Collections.Generic.List.SynchronizedList<ushort>
	// System.Collections.Generic.List<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.List<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.List<Loom.DelayedQueueItem>
	// System.Collections.Generic.List<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.List<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.List<System.DateTime>
	// System.Collections.Generic.List<System.Guid>
	// System.Collections.Generic.List<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.List<System.ValueTuple<int,object>>
	// System.Collections.Generic.List<System.ValueTuple<object,int>>
	// System.Collections.Generic.List<UnityEngine.Color32>
	// System.Collections.Generic.List<UnityEngine.Vector2>
	// System.Collections.Generic.List<byte>
	// System.Collections.Generic.List<float>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List<ushort>
	// System.Collections.Generic.ObjectComparer<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.ObjectComparer<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.ObjectComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.ObjectComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ObjectComparer<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.ObjectComparer<System.DateTime>
	// System.Collections.Generic.ObjectComparer<System.Guid>
	// System.Collections.Generic.ObjectComparer<System.Nullable<double>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<object,int>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Color32>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<double>
	// System.Collections.Generic.ObjectComparer<float>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<long>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectComparer<ushort>
	// System.Collections.Generic.ObjectEqualityComparer<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.Generic.ObjectEqualityComparer<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.Generic.ObjectEqualityComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.ObjectEqualityComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ObjectEqualityComparer<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.Generic.ObjectEqualityComparer<System.DateTime>
	// System.Collections.Generic.ObjectEqualityComparer<System.Guid>
	// System.Collections.Generic.ObjectEqualityComparer<System.IntPtr>
	// System.Collections.Generic.ObjectEqualityComparer<System.Nullable<double>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<int,object,double>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<object,int>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Color32>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<double>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<long>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<ulong>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Collections.ObjectModel.ReadOnlyCollection<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Collections.ObjectModel.ReadOnlyCollection<Loom.DelayedQueueItem>
	// System.Collections.ObjectModel.ReadOnlyCollection<Loom.NoDelayedQueueItem>
	// System.Collections.ObjectModel.ReadOnlyCollection<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.DateTime>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Guid>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<int,object,double>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<int,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<object,int>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Color32>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2>
	// System.Collections.ObjectModel.ReadOnlyCollection<byte>
	// System.Collections.ObjectModel.ReadOnlyCollection<float>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<long>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<ushort>
	// System.Comparison<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Comparison<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Comparison<Loom.DelayedQueueItem>
	// System.Comparison<Loom.NoDelayedQueueItem>
	// System.Comparison<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Comparison<System.DateTime>
	// System.Comparison<System.Guid>
	// System.Comparison<System.ValueTuple<int,object,double>>
	// System.Comparison<System.ValueTuple<int,object>>
	// System.Comparison<System.ValueTuple<object,int>>
	// System.Comparison<UnityEngine.Color32>
	// System.Comparison<UnityEngine.Vector2>
	// System.Comparison<byte>
	// System.Comparison<float>
	// System.Comparison<int>
	// System.Comparison<long>
	// System.Comparison<object>
	// System.Comparison<ushort>
	// System.Converter<object,object>
	// System.EventHandler<object>
	// System.Func<Loom.DelayedQueueItem,byte>
	// System.Func<PagSegment,byte>
	// System.Func<System.Guid,object,object>
	// System.Func<System.Guid,object>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<byte>
	// System.Func<int,byte>
	// System.Func<int,float>
	// System.Func<int,object>
	// System.Func<long,byte>
	// System.Func<object,PagSegment>
	// System.Func<object,System.Nullable<byte>>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,byte>
	// System.Func<object,float>
	// System.Func<object,int,object>
	// System.Func<object,int>
	// System.Func<object,long>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Func<ushort,byte>
	// System.Linq.Buffer<PagSegment>
	// System.Linq.Buffer<int>
	// System.Linq.Buffer<object>
	// System.Linq.Enumerable.<SelectIterator>d__5<object,object>
	// System.Linq.Enumerable.<SkipIterator>d__31<object>
	// System.Linq.Enumerable.<TakeIterator>d__25<int>
	// System.Linq.Enumerable.<TakeIterator>d__25<object>
	// System.Linq.Enumerable.Iterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.Iterator<PagSegment>
	// System.Linq.Enumerable.Iterator<int>
	// System.Linq.Enumerable.Iterator<long>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.WhereEnumerableIterator<PagSegment>
	// System.Linq.Enumerable.WhereEnumerableIterator<int>
	// System.Linq.Enumerable.WhereEnumerableIterator<long>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,PagSegment>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,long>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,PagSegment>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,long>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,PagSegment>
	// System.Linq.Enumerable.WhereSelectListIterator<object,int>
	// System.Linq.Enumerable.WhereSelectListIterator<object,long>
	// System.Linq.Enumerable.WhereSelectListIterator<object,object>
	// System.Linq.EnumerableSorter<int,float>
	// System.Linq.EnumerableSorter<int>
	// System.Linq.EnumerableSorter<object,float>
	// System.Linq.EnumerableSorter<object>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<int>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<object>
	// System.Linq.OrderedEnumerable<int,float>
	// System.Linq.OrderedEnumerable<int>
	// System.Linq.OrderedEnumerable<object,float>
	// System.Linq.OrderedEnumerable<object>
	// System.Nullable<UnityEngine.Color>
	// System.Nullable<byte>
	// System.Nullable<double>
	// System.Nullable<float>
	// System.Nullable<int>
	// System.Nullable<long>
	// System.Predicate<GameMaker.SlotGameRecordExport.SlotGameRecordCsvRow>
	// System.Predicate<GameMaker.SlotGameRecordExport.SlotGameRecordSummaryMetric>
	// System.Predicate<Loom.DelayedQueueItem>
	// System.Predicate<Loom.NoDelayedQueueItem>
	// System.Predicate<SlotMaker.ReelSettingModel.STReelSetting>
	// System.Predicate<System.DateTime>
	// System.Predicate<System.Guid>
	// System.Predicate<System.ValueTuple<int,object,double>>
	// System.Predicate<System.ValueTuple<int,object>>
	// System.Predicate<System.ValueTuple<object,int>>
	// System.Predicate<UnityEngine.Color32>
	// System.Predicate<UnityEngine.Vector2>
	// System.Predicate<byte>
	// System.Predicate<float>
	// System.Predicate<int>
	// System.Predicate<long>
	// System.Predicate<object>
	// System.Predicate<ushort>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task.<>c<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task.<>c<object>
	// System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_1<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_1<object>
	// System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory<object>
	// System.ValueTuple<double,System.Nullable<double>>
	// System.ValueTuple<int,int>
	// System.ValueTuple<int,object,double>
	// System.ValueTuple<int,object>
	// System.ValueTuple<object,int>
	// System.ValueTuple<object,object>
	// UnityEngine.Events.InvokableCall<byte>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.UnityAction<System.DateTime>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<int>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<object>
	// }}

	public void RefMethods()
	{
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// byte Newtonsoft.Json.Linq.Extensions.Convert<object,byte>(object)
		// double Newtonsoft.Json.Linq.Extensions.Convert<object,double>(object)
		// object Newtonsoft.Json.Linq.Extensions.Convert<object,object>(object)
		// byte Newtonsoft.Json.Linq.Extensions.Value<byte>(System.Collections.Generic.IEnumerable<Newtonsoft.Json.Linq.JToken>)
		// byte Newtonsoft.Json.Linq.Extensions.Value<object,byte>(System.Collections.Generic.IEnumerable<object>)
		// double Newtonsoft.Json.Linq.Extensions.Value<double>(System.Collections.Generic.IEnumerable<Newtonsoft.Json.Linq.JToken>)
		// double Newtonsoft.Json.Linq.Extensions.Value<object,double>(System.Collections.Generic.IEnumerable<object>)
		// object Newtonsoft.Json.Linq.Extensions.Value<object,object>(System.Collections.Generic.IEnumerable<object>)
		// object Newtonsoft.Json.Linq.Extensions.Value<object>(System.Collections.Generic.IEnumerable<Newtonsoft.Json.Linq.JToken>)
		// int Newtonsoft.Json.Linq.JToken.ToObject<int>()
		// object Newtonsoft.Json.Linq.JToken.ToObject<object>()
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.ConvertAll<object,object>(object[],System.Converter<object,object>)
		// UnityEngine.AnimatorClipInfo[] System.Array.Empty<UnityEngine.AnimatorClipInfo>()
		// object[] System.Array.Empty<object>()
		// ushort[] System.Array.Empty<ushort>()
		// int System.Array.IndexOf<object>(object[],object)
		// int System.Array.IndexOfImpl<object>(object[],object,int,int)
		// bool System.Enum.TryParse<int>(string,bool,int&)
		// bool System.Enum.TryParse<int>(string,int&)
		// bool System.Linq.Enumerable.All<int>(System.Collections.Generic.IEnumerable<int>,System.Func<int,bool>)
		// bool System.Linq.Enumerable.Contains<int>(System.Collections.Generic.IEnumerable<int>,int)
		// bool System.Linq.Enumerable.Contains<int>(System.Collections.Generic.IEnumerable<int>,int,System.Collections.Generic.IEqualityComparer<int>)
		// int System.Linq.Enumerable.Count<ushort>(System.Collections.Generic.IEnumerable<ushort>,System.Func<ushort,bool>)
		// System.Collections.Generic.KeyValuePair<object,float> System.Linq.Enumerable.ElementAt<System.Collections.Generic.KeyValuePair<object,float>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>,int)
		// System.Collections.Generic.KeyValuePair<object,object> System.Linq.Enumerable.ElementAt<System.Collections.Generic.KeyValuePair<object,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>,int)
		// System.Collections.Generic.KeyValuePair<ulong,float> System.Linq.Enumerable.ElementAt<System.Collections.Generic.KeyValuePair<ulong,float>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,float>>,int)
		// object System.Linq.Enumerable.ElementAt<object>(System.Collections.Generic.IEnumerable<object>,int)
		// int System.Linq.Enumerable.First<int>(System.Collections.Generic.IEnumerable<int>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Linq.IOrderedEnumerable<int> System.Linq.Enumerable.OrderBy<int,float>(System.Collections.Generic.IEnumerable<int>,System.Func<int,float>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,float>(System.Collections.Generic.IEnumerable<object>,System.Func<object,float>)
		// System.Collections.Generic.IEnumerable<PagSegment> System.Linq.Enumerable.Select<object,PagSegment>(System.Collections.Generic.IEnumerable<object>,System.Func<object,PagSegment>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<long> System.Linq.Enumerable.Select<object,long>(System.Collections.Generic.IEnumerable<object>,System.Func<object,long>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectIterator<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Skip<object>(System.Collections.Generic.IEnumerable<object>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SkipIterator<object>(System.Collections.Generic.IEnumerable<object>,int)
		// long System.Linq.Enumerable.Sum<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,long>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Take<int>(System.Collections.Generic.IEnumerable<int>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Take<object>(System.Collections.Generic.IEnumerable<object>,int)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.TakeIterator<int>(System.Collections.Generic.IEnumerable<int>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.TakeIterator<object>(System.Collections.Generic.IEnumerable<object>,int)
		// PagSegment[] System.Linq.Enumerable.ToArray<PagSegment>(System.Collections.Generic.IEnumerable<PagSegment>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<int> System.Linq.Enumerable.ToList<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<Loom.DelayedQueueItem> System.Linq.Enumerable.Where<Loom.DelayedQueueItem>(System.Collections.Generic.IEnumerable<Loom.DelayedQueueItem>,System.Func<Loom.DelayedQueueItem,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<PagSegment> System.Linq.Enumerable.Iterator<object>.Select<PagSegment>(System.Func<object,PagSegment>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<object>.Select<int>(System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<long> System.Linq.Enumerable.Iterator<object>.Select<long>(System.Func<object,long>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,InOutRecordController.<WaitUntil>d__27>(System.Runtime.CompilerServices.TaskAwaiter&,InOutRecordController.<WaitUntil>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PageBase.<WaitUntil>d__29>(System.Runtime.CompilerServices.TaskAwaiter&,PageBase.<WaitUntil>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PageManager.<WaitUntil>d__15>(System.Runtime.CompilerServices.TaskAwaiter&,PageManager.<WaitUntil>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleTableUtils.<CheckOrCreatTableBet>d__4>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleTableUtils.<CheckOrCreatTableBet>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OnClickMaxGameRecordManualInputAsync>d__32>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OnClickMaxGameRecordManualInputAsync>d__32&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,InOutRecordController.<WaitUntil>d__27>(System.Runtime.CompilerServices.TaskAwaiter&,InOutRecordController.<WaitUntil>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PageBase.<WaitUntil>d__29>(System.Runtime.CompilerServices.TaskAwaiter&,PageBase.<WaitUntil>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PageManager.<WaitUntil>d__15>(System.Runtime.CompilerServices.TaskAwaiter&,PageManager.<WaitUntil>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleTableUtils.<CheckOrCreatTableBet>d__4>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleTableUtils.<CheckOrCreatTableBet>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OnClickMaxGameRecordManualInputAsync>d__32>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OnClickMaxGameRecordManualInputAsync>d__32&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PageBase.<OnOpenAsync>d__25>(System.Runtime.CompilerServices.TaskAwaiter&,PageBase.<OnOpenAsync>d__25&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PageManager.<OpenPageAsync>d__11>(System.Runtime.CompilerServices.TaskAwaiter&,PageManager.<OpenPageAsync>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,PageManager.<OpenPageAsync>d__11>(System.Runtime.CompilerServices.TaskAwaiter<object>&,PageManager.<OpenPageAsync>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager02.<LoadAssetAsync>d__15<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager02.<LoadAssetAsync>d__15<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ResourceManager02.<LoadAssetBundleAsync>d__16>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ResourceManager02.<LoadAssetBundleAsync>d__16&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,SQLitePlayerPrefs03.<GetData>d__28>(System.Runtime.CompilerServices.TaskAwaiter<object>&,SQLitePlayerPrefs03.<GetData>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,StreamingAssetsBundleLoader.<LoadAssetAsync>d__12<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,StreamingAssetsBundleLoader.<LoadAssetAsync>d__12<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TableBetItem.<DefaultTable>d__10>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TableBetItem.<DefaultTable>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,StreamingAssetsBundleLoader.<LoadAssetBundleAsync>d__13>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,StreamingAssetsBundleLoader.<LoadAssetBundleAsync>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<ConsoleTableUtils.<CheckOrCreatTableBet>d__4>(ConsoleTableUtils.<CheckOrCreatTableBet>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<InOutRecordController.<WaitUntil>d__27>(InOutRecordController.<WaitUntil>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PageBase.<WaitUntil>d__29>(PageBase.<WaitUntil>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PageManager.<WaitUntil>d__15>(PageManager.<WaitUntil>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<TabSettingsMachineController.<OnClickMaxGameRecordManualInputAsync>d__32>(TabSettingsMachineController.<OnClickMaxGameRecordManualInputAsync>d__32&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<PageBase.<OnOpenAsync>d__25>(PageBase.<OnOpenAsync>d__25&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<PageManager.<OpenPageAsync>d__11>(PageManager.<OpenPageAsync>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<ResourceManager02.<LoadAssetAsync>d__15<object>>(ResourceManager02.<LoadAssetAsync>d__15<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<ResourceManager02.<LoadAssetBundleAsync>d__16>(ResourceManager02.<LoadAssetBundleAsync>d__16&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<SQLitePlayerPrefs03.<GetData>d__28>(SQLitePlayerPrefs03.<GetData>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<StreamingAssetsBundleLoader.<LoadAssetAsync>d__12<object>>(StreamingAssetsBundleLoader.<LoadAssetAsync>d__12<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<StreamingAssetsBundleLoader.<LoadAssetBundleAsync>d__13>(StreamingAssetsBundleLoader.<LoadAssetBundleAsync>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<TableBetItem.<DefaultTable>d__10>(TableBetItem.<DefaultTable>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,InOutRecordController.<SetInOutTotal>d__26>(System.Runtime.CompilerServices.TaskAwaiter&,InOutRecordController.<SetInOutTotal>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,TabSettingsMachineController.<OpenMaxGameRecordExpandMenu>d__29>(System.Runtime.CompilerServices.TaskAwaiter&,TabSettingsMachineController.<OpenMaxGameRecordExpandMenu>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleSlot01.ChangePasswordController.<<OnClickSetPassword>b__12_0>d>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleSlot01.ChangePasswordController.<<OnClickSetPassword>b__12_0>d&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleSlot01.ChangePasswordController.<OnClickSetPassword>d__12>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleSlot01.ChangePasswordController.<OnClickSetPassword>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleSlot01.PageConsoleMain.<OnChenkUser>d__24>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleSlot01.PageConsoleMain.<OnChenkUser>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleSlot01.PageConsoleMain.<OnClickLanguage>d__39>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleSlot01.PageConsoleMain.<OnClickLanguage>d__39&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleSlot01.PageConsoleMain.<OnClickTimeDate>d__35>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleSlot01.PageConsoleMain.<OnClickTimeDate>d__35&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleSlot01.PopupConsoleSlideSetting.<OnClickKeyboard>d__25>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleSlot01.PopupConsoleSlideSetting.<OnClickKeyboard>d__25&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,ConsoleTableUtils.<EnsureBetRowInsertDefaultAsync>d__15>(System.Runtime.CompilerServices.TaskAwaiter<object>&,ConsoleTableUtils.<EnsureBetRowInsertDefaultAsync>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,DeviceCoder.<OnResponseShowCoder>d__11>(System.Runtime.CompilerServices.TaskAwaiter<object>&,DeviceCoder.<OnResponseShowCoder>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,PageManager.<OpenPageAsync>d__10>(System.Runtime.CompilerServices.TaskAwaiter<object>&,PageManager.<OpenPageAsync>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsInOutController.<OnClickBillValidatorModel>d__19>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsInOutController.<OnClickBillValidatorModel>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsInOutController.<OnClickCoinInScale>d__14>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsInOutController.<OnClickCoinInScale>d__14&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsInOutController.<OnClickCoinOutScale>d__13>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsInOutController.<OnClickCoinOutScale>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsInOutController.<OnClickPrinterModel>d__18>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsInOutController.<OnClickPrinterModel>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsInOutController.<OnClickScoreScale>d__15>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsInOutController.<OnClickScoreScale>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OnClickAgentIDMachineID>d__24>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OnClickAgentIDMachineID>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OnClickMaxBusinessDayRecord>d__35>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OnClickMaxBusinessDayRecord>d__35&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OnClickMaxCoinInOutRecord>d__26>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OnClickMaxCoinInOutRecord>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OnClickMaxErrorRecord>d__34>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OnClickMaxErrorRecord>d__34&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OnClickMaxEventRecord>d__33>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OnClickMaxEventRecord>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,TabSettingsMachineController.<OpenMaxGameRecordExpandMenu>d__29>(System.Runtime.CompilerServices.TaskAwaiter<object>&,TabSettingsMachineController.<OpenMaxGameRecordExpandMenu>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Test02.<OpenABConsoleMainPage>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Test02.<OpenABConsoleMainPage>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Test02.<ShowJson>d__9>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Test02.<ShowJson>d__9&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ConsoleSlot01.ChangePasswordController.<<OnClickSetPassword>b__12_0>d>(ConsoleSlot01.ChangePasswordController.<<OnClickSetPassword>b__12_0>d&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ConsoleSlot01.ChangePasswordController.<OnClickSetPassword>d__12>(ConsoleSlot01.ChangePasswordController.<OnClickSetPassword>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ConsoleSlot01.PageConsoleMain.<OnChenkUser>d__24>(ConsoleSlot01.PageConsoleMain.<OnChenkUser>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ConsoleSlot01.PageConsoleMain.<OnClickLanguage>d__39>(ConsoleSlot01.PageConsoleMain.<OnClickLanguage>d__39&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ConsoleSlot01.PageConsoleMain.<OnClickTimeDate>d__35>(ConsoleSlot01.PageConsoleMain.<OnClickTimeDate>d__35&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ConsoleSlot01.PopupConsoleSlideSetting.<OnClickKeyboard>d__25>(ConsoleSlot01.PopupConsoleSlideSetting.<OnClickKeyboard>d__25&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<ConsoleTableUtils.<EnsureBetRowInsertDefaultAsync>d__15>(ConsoleTableUtils.<EnsureBetRowInsertDefaultAsync>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<DeviceCoder.<OnResponseShowCoder>d__11>(DeviceCoder.<OnResponseShowCoder>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<InOutRecordController.<SetInOutTotal>d__26>(InOutRecordController.<SetInOutTotal>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<PageManager.<OpenPageAsync>d__10>(PageManager.<OpenPageAsync>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsInOutController.<OnClickBillValidatorModel>d__19>(TabSettingsInOutController.<OnClickBillValidatorModel>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsInOutController.<OnClickCoinInScale>d__14>(TabSettingsInOutController.<OnClickCoinInScale>d__14&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsInOutController.<OnClickCoinOutScale>d__13>(TabSettingsInOutController.<OnClickCoinOutScale>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsInOutController.<OnClickPrinterModel>d__18>(TabSettingsInOutController.<OnClickPrinterModel>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsInOutController.<OnClickScoreScale>d__15>(TabSettingsInOutController.<OnClickScoreScale>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsMachineController.<OnClickAgentIDMachineID>d__24>(TabSettingsMachineController.<OnClickAgentIDMachineID>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsMachineController.<OnClickMaxBusinessDayRecord>d__35>(TabSettingsMachineController.<OnClickMaxBusinessDayRecord>d__35&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsMachineController.<OnClickMaxCoinInOutRecord>d__26>(TabSettingsMachineController.<OnClickMaxCoinInOutRecord>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsMachineController.<OnClickMaxErrorRecord>d__34>(TabSettingsMachineController.<OnClickMaxErrorRecord>d__34&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsMachineController.<OnClickMaxEventRecord>d__33>(TabSettingsMachineController.<OnClickMaxEventRecord>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<TabSettingsMachineController.<OpenMaxGameRecordExpandMenu>d__29>(TabSettingsMachineController.<OpenMaxGameRecordExpandMenu>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Test02.<OpenABConsoleMainPage>d__5>(Test02.<OpenABConsoleMainPage>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Test02.<ShowJson>d__9>(Test02.<ShowJson>d__9&)
		// byte UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<byte>(System.IntPtr)
		// float UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<float>(System.IntPtr)
		// int UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<int>(System.IntPtr)
		// long UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<long>(System.IntPtr)
		// object UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetFieldID<long>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<byte>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<float>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<int>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<long>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<object>(System.IntPtr,string,object[],bool)
		// byte UnityEngine.AndroidJavaObject.Call<byte>(string,object[])
		// int UnityEngine.AndroidJavaObject.Call<int>(string,object[])
		// object UnityEngine.AndroidJavaObject.Call<object>(string,object[])
		// byte UnityEngine.AndroidJavaObject.CallStatic<byte>(string,object[])
		// float UnityEngine.AndroidJavaObject.CallStatic<float>(string,object[])
		// int UnityEngine.AndroidJavaObject.CallStatic<int>(string,object[])
		// long UnityEngine.AndroidJavaObject.CallStatic<long>(string,object[])
		// long UnityEngine.AndroidJavaObject.Get<long>(string)
		// object UnityEngine.AndroidJavaObject.GetStatic<object>(string)
		// byte UnityEngine.AndroidJavaObject._Call<byte>(string,object[])
		// int UnityEngine.AndroidJavaObject._Call<int>(string,object[])
		// object UnityEngine.AndroidJavaObject._Call<object>(string,object[])
		// byte UnityEngine.AndroidJavaObject._CallStatic<byte>(string,object[])
		// float UnityEngine.AndroidJavaObject._CallStatic<float>(string,object[])
		// int UnityEngine.AndroidJavaObject._CallStatic<int>(string,object[])
		// long UnityEngine.AndroidJavaObject._CallStatic<long>(string,object[])
		// long UnityEngine.AndroidJavaObject._Get<long>(string)
		// object UnityEngine.AndroidJavaObject._GetStatic<object>(string)
		// object UnityEngine.AssetBundle.LoadAsset<object>(string)
		// UnityEngine.AssetBundleRequest UnityEngine.AssetBundle.LoadAssetAsync<object>(string)
		// object UnityEngine.Component.GetComponent<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.Component.TryGetComponent<object>(object&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// object UnityEngine.JsonUtility.FromJson<object>(string)
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object[] UnityEngine.Object.FindObjectsOfType<object>(bool)
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object[] UnityEngine.Resources.ConvertObjects<object>(UnityEngine.Object[])
		// object[] UnityEngine.Resources.FindObjectsOfTypeAll<object>()
		// object UnityEngine.Resources.Load<object>(string)
		// byte UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<byte>(System.IntPtr)
		// float UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<float>(System.IntPtr)
		// int UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<int>(System.IntPtr)
		// long UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<long>(System.IntPtr)
		// object UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetFieldID<long>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<byte>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<float>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<int>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<long>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<object>(System.IntPtr,string,object[],bool)
		// string UnityEngine._AndroidJNIHelper.GetSignature<byte>(object[])
		// string UnityEngine._AndroidJNIHelper.GetSignature<float>(object[])
		// string UnityEngine._AndroidJNIHelper.GetSignature<int>(object[])
		// string UnityEngine._AndroidJNIHelper.GetSignature<long>(object[])
		// string UnityEngine._AndroidJNIHelper.GetSignature<object>(object[])
	}
}