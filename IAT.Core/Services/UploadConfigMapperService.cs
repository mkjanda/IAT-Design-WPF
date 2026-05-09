using IAT.Core.ConfigFile;
using IAT.Core.Enumerations;
using IAT.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services
{
    public interface IUploadConfigMapperService
    {
        SerializableIatConfig MapToServerConfig(IatTest iat);
    }

    internal class UploadConfigMapperService
    {
        private readonly ILocalStorageService _localStorage;
        public UploadConfigMapperService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }
        public SerializableIatConfig MapToServerConfig(IatTest iat)
        {
            return new SerializableIatConfig()
            {
                IATName = iat.Name,
                Author = _localStorage[Field.UserName],
                NumIATItems = iat.Trials.Count,
                Is7Block = true,
                LeftResponseKey = 'E',
                RightResponseKey = 'I',




            };,

        }
    }
}
