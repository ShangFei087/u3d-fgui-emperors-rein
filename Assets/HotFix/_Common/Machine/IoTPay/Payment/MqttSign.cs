using System;
using System.Security.Cryptography;

namespace aiot_paho_csharp
{
    class CryptoUtil
    {
        public static String hmacSha256(String plainText, String key)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] plainTextBytes = encoding.GetBytes(plainText);
            byte[] keyBytes = encoding.GetBytes(key);

            HMACSHA256 hmac = new HMACSHA256(keyBytes);
            byte[] sign = hmac.ComputeHash(plainTextBytes);
            return BitConverter.ToString(sign).Replace("-", string.Empty);
        }
    }
    public class MqttSign
    {
        private String username = "";

        private String password = "";

        private String clientid = "";

        public String getUsername() { return this.username; }

        public String getPassword() { return this.password; }

        public String getClientid() { return this.clientid; }

        public bool calculate(String productKey, String deviceName, String deviceSecret)
        {
            if (productKey == null || deviceName == null || deviceSecret == null)
            {
                return false;
            }

            //MQTT用户名
            this.username = deviceName + "&" + productKey;

            //MQTT密码
            String timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
            String plainPasswd = "clientId" + productKey + "." + deviceName + "deviceName" +
                    deviceName + "productKey" + productKey + "timestamp" + timestamp;
            this.password = CryptoUtil.hmacSha256(plainPasswd, deviceSecret);

            //MQTT ClientId
            this.clientid = productKey + "." + deviceName + "|" + "timestamp=" + timestamp +
                    ",_v=paho-c#-1.0.0,securemode=2,signmethod=hmacsha256|";

            return true;
        }
    }
}