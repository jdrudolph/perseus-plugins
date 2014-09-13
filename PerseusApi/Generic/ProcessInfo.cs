using System;
using System.Collections.Generic;
using System.Linq;
using BaseLibS.Util;

namespace PerseusApi.Generic{
	public class ProcessInfo{
		private readonly int numThreads;
		public Settings Settings { get; private set; }
		public Action<string> Status { get; private set; }
		public Action<int> Progress { get; private set; }
		public Action<int> ReduceThreads { get; private set; }
		public string ErrString { get; set; }
		public List<ThreadDistributor> threadDistributors = new List<ThreadDistributor>();

		public ProcessInfo(Settings settings, Action<string> status, Action<int> progress, int numThreads,
			Action<int> reduceThreads){
			Settings = settings;
			Status = status;
			Progress = progress;
			this.numThreads = numThreads;
			ReduceThreads = reduceThreads;
		}

		public int NumThreads { get { return Math.Min(numThreads, Settings.Nthreads); } }

		public void Abort(){
			foreach (
				ThreadDistributor threadDistributor in threadDistributors.Where(threadDistributor => threadDistributor != null)){
				threadDistributor.Abort();
			}
		}
	}
}