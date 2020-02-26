using UnityEngine;
using UnityEngine.UI;

public class ItemsCanvasBehavior : MonoBehaviour
{
    public Image item1Image;
    public Text item1CountText;

    public Image item2Image;
    public Text item2CountText;

    public Image item3Image;
    public Text item3CountText;

    public Image item4Image;
    public Text item4CountText;

    public Image item5Image;
    public Text item5CountText;

    public Image item6Image;
    public Text item6CountText;

    public Image item7Image;
    public Text item7CountText;

    public Image item8Image;
    public Text item8CountText;

    private Image[] itemImages;
    private Text[] itemTexts;

    public void Initialize()
    {
        itemImages = new Image[]
        {
            item1Image,
            item2Image,
            item3Image,
            item4Image,
            item5Image,
            item6Image,
            item7Image,
            item8Image,
        };

        itemTexts = new Text[]
        {
            item1CountText,
            item2CountText,
            item3CountText,
            item4CountText,
            item5CountText,
            item6CountText,
            item7CountText,
            item8CountText
        };

        // Hide everything except for those that the level wants to init
        for (int i = 0; i < itemImages.Length; ++i)
        {
            itemImages[i].gameObject.SetActive(false);
            itemTexts[i].gameObject.SetActive(false);
        }
    }

    public void InitItemNumber(int itemNumber, Sprite sprite)
    {
        if (itemNumber >= 0 && itemNumber < 8)
        {
            itemImages[itemNumber].sprite = sprite;
            itemImages[itemNumber].gameObject.SetActive(true);
            itemTexts[itemNumber].gameObject.SetActive(true);

            itemTexts[itemNumber].text = "0";
        }
    }

    public void SetItemNumberText(int itemNumber, string text)
    {
        if (itemNumber >= 0 && itemNumber < 98)
        {
            itemTexts[itemNumber].text = text;
        }
    }
}
