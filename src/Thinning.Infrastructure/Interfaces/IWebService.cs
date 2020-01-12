﻿namespace Thinning.Infrastructure.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Thinning.Infrastructure.Models;
    
    public interface IWebService
    {
        void UpdateStorage(List<string> algorithmNames);

        void UpdateStorage(TestResult testResul, string baseImageFilepath);

        Task<bool> PublishResults();
    }
}
