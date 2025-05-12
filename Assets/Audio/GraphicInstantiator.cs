using UnityEngine;

public class GraphicInstantiator : MonoBehaviour
{
    public GameObject _sampleCubePrefab;
    GameObject[] _sampleCube = new GameObject[512];
    public float _maxScale;
    
    void Start()
    {
        for(int i=0;i<512;i++)
        {
            GameObject _instanceSampleCube = (GameObject)Instantiate(_sampleCubePrefab);
            _instanceSampleCube.transform.position = this.transform.position;
            _instanceSampleCube.transform.parent = this.transform; 
            _instanceSampleCube.transform.name = "SampleCube" + i.ToString();
            this.transform.eulerAngles = new Vector3(0, -0.703125f * i, 0);
            _instanceSampleCube.transform.position = Vector3.forward * 100;
            _sampleCube[i] = _instanceSampleCube;
        }
    }

    
    void Update()
    {
        for(int i=0;i<512;i++)
        {
            if(_sampleCube != null)
            {
                _sampleCube[i].transform.localScale = new Vector3(10, ((AudioPeer._samplesLeft[i] + AudioPeer._samplesRight[i]) * _maxScale) + 2, 10);
            }
        }
    }
}
