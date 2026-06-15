using UnityEngine;
using TMPro;

namespace Assets.Scripts.CreateMap.UI
{
    public class UIGenerateMapInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputMapSize;
        [SerializeField] private TMP_InputField inputClusterSize;

        public void GetInput(out int mapSize, out int clusterSize)
        {
            var input = inputMapSize.text;
            mapSize = StringToInt(input);

            input = inputClusterSize.text;
            clusterSize = StringToInt(input);
        }

        public int GetMapSize()
        {
            var input = inputMapSize.text;
            return StringToInt(input);
        }
        public int GetClusterSize()
        {
            var input = inputClusterSize.text;
            return StringToInt(input);
        }

        public int StringToInt(string text)
        {
            if (text == null || text.Length == 0) return 0;

            int i = 0;
            while (text[i] <= ' ')
            {
                if (text[i] == 0) return 0;
                i++;
            }

            int result = 0;
            for (; i < text.Length; i++)
            {
                result = result * 10 + (text[i] - '0');
            }
            return result;
        }
    }
}
