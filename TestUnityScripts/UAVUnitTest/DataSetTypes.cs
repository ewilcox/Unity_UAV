namespace UAVUnitTest
{
    public class LocPSTest
    {
    }
    public class CrowdPSTest
    {
    }
    public class PEITest
    {
        public float min;
        public float max;
        public float response;
        public PEITest(float _max, float _min, float _response)
        {
            min = _min;
            max = _max;
            response = _response;
        }
    }
    public class AEDTest
    {
        public float posX, posY, posZ;
        public float responseX, responseY, responseZ;
        public AEDTest(float _posX, float _posY, float _posZ, float _responseX, float _responseY, float _responseZ)
        {
            posX = _posX;
            posY = _posY;
            posZ = _posZ;
            responseX = _responseX;
            responseY = _responseY;
            responseZ = _responseZ;
        }
    }
    public class REITest
    {
        public float posX, posY, posZ;
        public float responseX, responseY, responseZ;
        public REITest(float _posX, float _posY, float _posZ, float _responseX, float _responseY, float _responseZ)
        {
            posX = _posX;
            posY = _posY;
            posZ = _posZ;
            responseX = _responseX;
            responseY = _responseY;
            responseZ = _responseZ;
        }
    }
}