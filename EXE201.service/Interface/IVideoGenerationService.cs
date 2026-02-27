using EXE201.Service.DTOs.GeminiDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IVideoGenerationService
    {
        /// <summary>
        /// Generate video using outfit images as reference
        /// </summary>
        Task<VideoGenerationResponse> GenerateVideoAsync(VideoGenerationRequest request);

        /// <summary>
        /// Check the status of video generation operation
        /// </summary>
        Task<VideoStatusResponse> GetVideoStatusAsync(string operationName);

        /// <summary>
        /// Download the generated video
        /// </summary>
        Task<byte[]> DownloadVideoAsync(string videoUri);
    }
}
