using Ekr.Core.Securities.Symmetric;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ekr.Core.Entities
{
    public class ServiceResponseEncrypted<T>
    {
        public const string AESKEY = "9IO5BHL8G3NJRQ35L5B3NI67LJ25DVES";
        public T Data;

        public string x
        {
            get
            {
                var json = JsonConvert.SerializeObject(Data);

                string jsonEncrypted = Aes256Encryption.Encrypt(json, AESKEY);
                string jsonDecryptMethod = Aes256Encryption.Decrypt(jsonEncrypted, AESKEY);
                string jsonDecrypt = Aes256Encryption.DecryptString("tiQbA2K2ALNpvmaijNjdjCn3u3M8phVRGelTHE0afletjcorCt4FuF1taJ6XBStgwlXP60q4ARXOZl2kEw6Roz81MzABT3FIskaCs4B4/mNVd5gh/MuyphBh1ECOQwefSgvXLdIswgLF3xBRdb0bSJm2PYI61cWKWb5lFY8EZEdXqk2a3zr7EOOBqnSXwKKuVjEMim3V6H3iPNlASrY+GlbbRZB98Af7qren36xekVr5m2rypFA74JTM1RlwKzGoxEk+YK9x3Z1kHgjDLvJ7N+4AZ8D1pqrP5GEcz/rFaAi4W1jhG3dt6x4A/v4LA+6HWedPtaF3ErlcSdr7eN9jGt152jLnbu/r5cM0+Ron+VNkCoUHyNfjIiGniV6UgaBi/6loOufoSRwXZrgmVK3LBcVzVmEExHFw/rBqO1QqcZIqVf/6724UG+MVxk/k7+01ia5RucBXku9/Jcoy9pwpRp8lTKgxvew5GAziRfqEf/gg8nF7QfMXvfXFSJ9BpaMjZmIG4NqraXMw/KpuWgHUQoVA9xT6zY0cWu24h1u/Gk0=");

                return Aes256Encryption.Encrypt(json, AESKEY);
            }
        }

    }
}
