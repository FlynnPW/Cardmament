using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardLoader : MonoBehaviour
{
    private string cardFolder = Application.dataPath + "\\Cards";
    private CardManager cardManager;
    [SerializeField]
    private Sprite[] imagesToLoad;
    private Dictionary<string, Sprite> images;
    const string cardFileFormat = ".txt";
    const string cardFileIdAttribute = "id";
    const string cardFileNameAttribute = "name";
    const string cardFileDescriptionAttribute = "description";
    const string cardFileImageAttribute = "image";
    const string cardFileManaCostAttribute = "mana_cost";
    const string cardFileImpactAttribute = "card_impact";
    //card move unit impact
    const string cardFileImpactMoveUnit = "moveUnit";
    const string cardFileImpactMoveDistanceAttribute = "move_distance";

    void Start()
    {
        cardManager = GetComponent<CardManager>();
        populateImageDictionary();
        loadCards();
    }

    void populateImageDictionary()
    {
        images = new Dictionary<string, Sprite>();

        foreach (Sprite image in imagesToLoad)
        {
            images.Add(image.name, image);
        }
    }

    void loadCards()
    {
        string[] files = Directory.GetFiles(cardFolder);
        Card[] cards = new Card[files.Length / 2]; //half to include meta files

        foreach(string file in files)
        {
            if (Path.GetExtension(file) == cardFileFormat)
            {
                TextFileAsset cardTextFile = new TextFileAsset(File.ReadAllLines(file));
                string idString = cardTextFile.getAttribute(cardFileIdAttribute);
                int id;

                if (int.TryParse(idString, out id) == false)
                {
                    Debug.LogError(idString + " is not a valid value for " + cardFileIdAttribute + " should be of type int");
                }

                string name = cardTextFile.getAttribute(cardFileNameAttribute);
                string description = cardTextFile.getAttribute(cardFileDescriptionAttribute);
                string imageString = cardTextFile.getAttribute(cardFileImageAttribute);
                Sprite image = null;

                if (images.ContainsKey(imageString) == false)
                {
                    Debug.LogError(imageString + " is not a image");
                }
                else
                {
                    image = images[imageString];
                }

                string cardManaCostString = cardTextFile.getAttribute(cardFileManaCostAttribute);
                int cardManaCost;

                if (int.TryParse(cardManaCostString, out cardManaCost) == false)
                {
                    Debug.LogError(cardManaCost + " is not a valid value for " + cardFileManaCostAttribute + " should be of type int");
                }

                string cardImpactString = cardTextFile.getAttribute(cardFileImpactAttribute);
                CardImpact cardImpact = null;

                switch (cardImpactString)
                {
                    case cardFileImpactMoveUnit:
                        string moveDistanceString = cardTextFile.getAttribute(cardFileImpactMoveDistanceAttribute);
                        int moveDistance;

                        if (int.TryParse(moveDistanceString, out moveDistance) == false)
                        {
                            Debug.LogError(moveDistanceString + " is not a valid value for " + cardFileImpactMoveDistanceAttribute + " should be of type int");
                            break;
                        }

                        cardImpact = new CardMoveUnitImpact(moveDistance);
                        break;
                    default:
                        Debug.LogError(cardImpactString + " is not a valid type of card impact");
                        break;
                }

                Card card = new Card(name, description, image, cardImpact, cardManaCost);
                cards[id] = card;
            }
        }

        cardManager.recieveCards(cards);
    }

    class TextFileAsset
    {
        private Dictionary<string, string> attributes;

        public TextFileAsset(string[] lines)
        {
            attributes = new Dictionary<string, string>();
            loadAttributes(lines);
        }

        private void loadAttributes(string[] lines)
        {
            foreach (string line in lines)
            {
                string[] splitLine = line.Split('=');
                string attribute = splitLine[0];
                string value = splitLine[1];

                attributes.Add(attribute, value);
            }
        }

        public string getAttribute(string attribute)
        {
            if (attributes.ContainsKey(attribute) == false)
            {
                return "";
            }

            return attributes[attribute];
        }
    }
}
