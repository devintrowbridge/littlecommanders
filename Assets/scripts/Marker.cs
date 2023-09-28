using UnityEngine;

public class Marker : MonoBehaviour {
    public float spacing;
    [SerializeField] FormPos pos;

    private void Awake()
    {
        pos = new FormPos();
    }

    public int rank
    {
        get => pos.rank;
        private set => SetPosition(value, pos.file);
    }

    public int file
    {
        get => pos.file;
        private set => SetPosition(pos.rank, value);
    }

    public void SetPosition(int rank, int file)
    {
        pos.rank = rank;
        pos.file = file;
        transform.localPosition = -spacing * new Vector3(file, 0, rank);
        Debug.Log(transform.localPosition);
    }

    public void SetPosition(FormPos pos)
    {
        SetPosition(pos.rank, pos.file);
    }

    public void RankFileFromPos()
    {
        pos.rank = Mathf.RoundToInt(Mathf.Abs(transform.localPosition.z / spacing));
        pos.file = Mathf.RoundToInt(Mathf.Abs(transform.localPosition.x / spacing));
    }
}
