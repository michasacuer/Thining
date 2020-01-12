﻿namespace Thinning.Infrastructure
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Thinning.Infrastructure.Interfaces;
    using Thinning.Infrastructure.Models;
    using static Thinning.Infrastructure.Models.Storage;

    public class WebService : IWebService
    {
        private readonly ISystemInfo systemInfo;

        public WebService(ISystemInfo systemInfo)
        {
            this.systemInfo = systemInfo;
        }

        public void UpdateStorage(List<string> algorithmNames)
        {
            StorageDto.TestLines = new List<TestLine>();
            foreach (string name in algorithmNames)
            {
                StorageDto.TestLines.Add(new TestLine
                {
                    AlgorithmName = name
                });
            }
        }

        public void UpdateStorage(TestResult testResult, string baseImageFilepath)
        {
            StorageDto.PcInfo = new PcInfo
            {
                Cpu = this.systemInfo.GetCpuInfo(),
                Gpu = this.systemInfo.GetGpuInfo(),
                Memory = this.systemInfo.GetTotalMemory(),
                Os = this.systemInfo.GetOperativeSystemInfo()
            };

            int algorithmCount = 0;
            foreach (var times in testResult.ResultTimes)
            {
                StorageDto.TestLines[algorithmCount].Iterations = times.Count;
                StorageDto.TestLines[algorithmCount].AlgorithmTestRuns = new List<TestRun>();
                
                int runCount = 0;
                foreach (double time in times)
                {
                    StorageDto.TestLines[algorithmCount].AlgorithmTestRuns.Add(new TestRun
                    {
                        Time = time,
                        RunCount = runCount
                    });

                    runCount++;
                }
            }

            StorageDto.Images = new List<Storage.Image>();
            int imageCount = 0;
            foreach (var bmp in testResult.ResultBitmaps)
            {
                StorageDto.Images.Add(new Storage.Image
                {
                    OriginalBpp = 0,
                    OriginalHeight = bmp.Height,
                    OriginalWidth = bmp.Width,
                    ImageContent = testResult.RawBitmaps[imageCount],
                    TestImage = false,
                    AlgorithmName = StorageDto.TestLines[imageCount].AlgorithmName
                });

                imageCount++;
            }

            var testBitmap = new Bitmap(baseImageFilepath);
            var bitmapData = testBitmap.LockBits(
                new Rectangle(0, 0, testBitmap.Width, testBitmap.Height),
                ImageLockMode.ReadWrite,
                testBitmap.PixelFormat);

            int pixelsCount = bitmapData.Stride * testBitmap.Height;
            var imageContent = new byte[pixelsCount];
            Marshal.Copy(bitmapData.Scan0, imageContent, 0, pixelsCount);
            testBitmap.UnlockBits(bitmapData);

            StorageDto.Images.Add(new Storage.Image
            {
                OriginalBpp = 0,
                OriginalHeight = testBitmap.Height,
                OriginalWidth = testBitmap.Width,
                ImageContent = imageContent,
                TestImage = true,
                AlgorithmName = "TestImage"
            });
        }
    }
}