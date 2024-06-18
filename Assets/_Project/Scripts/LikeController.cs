using UnityEngine;
using UnityEngine.UI;

public class LikeController : MonoBehaviour
{
    public int LikesCount = 0;
    public Text TextObject;


    void Update()
    {
        TextObject.text = LikesCount.ToString();
    }

    public void AddLike()
    {
        LikesCount = LikesCount + 1;
    }

    public void RemoveLike()
    {
        LikesCount = LikesCount - 1;
    }
}
