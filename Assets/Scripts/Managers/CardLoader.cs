using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using Mono.Data.Sqlite;
using Unity.VisualScripting;
using System;

public class CardLoader : MonoBehaviour
{
    private string cardFolder = Application.dataPath + "\\Cards";
    private CardManager cardManager;
    [SerializeField]
    private Sprite[] imagesToLoad;
    private Dictionary<string, Sprite> images;
    const string databaseName = "URI=file:Cards.db";
    const string cardFileFormat = ".txt";
    const string cardFileIdAttribute = "cardId";
    const string cardFileNameAttribute = "cardName";
    const string cardFileDescriptionAttribute = "cardDescription";
    const string cardFileImageAttribute = "image";
    const string cardFileManaCostAttribute = "manaCost";
    const string cardFileImpactAttribute = "card_impact";
    const string cardCountColumnName = "cardCount";
    //card move unit impact
    const string cardMoveUnitForeignKeyColumnName = "cardMoveUnitId";
    const string cardFileImpactMoveUnit = "moveUnit";
    const string cardFileImpactMoveDistanceAttribute = "moveDistance";
    //card create unit impact
    const string cardCreateUnitForeignKeyColumnName = "cardCreateUnitId";

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
        Card[] cards = null;

        using (var connection = new SqliteConnection(databaseName))
        {
            connection.Open();

            int getCountOfTable(string table)
            {
                var countCommand = connection.CreateCommand();
                countCommand.CommandText = "SELECT COUNT(*) AS " + cardCountColumnName + " FROM " + table;

                IDataReader countReader = countCommand.ExecuteReader();

                countReader.Read();
                int cardCount = int.Parse(countReader[cardCountColumnName].ToString());
                countReader.Close();
                return cardCount;
            }

            int cardCount = getCountOfTable("cards");
            print(cardCount);
            cards = new Card[cardCount];

            void populateCardArray()
            {
                var cardCommand = connection.CreateCommand();
                cardCommand.CommandText = "SELECT * FROM cards JOIN cardMainAttributes ON cards.cardMainAttributesId == cardMainAttributes.cardAttributesId LEFT JOIN moveUnitCards ON cards.cardMoveUnitId == moveUnitCards.cardImpactId LEFT JOIN createUnitCards ON cards.cardCreateUnitId == createUnitCards.CardImpactId";
                IDataReader reader = cardCommand.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt16(0);
                    string name = reader[cardFileNameAttribute].ToString();
                    string description = reader[cardFileDescriptionAttribute].ToString();
                    string imageString = reader[cardFileImageAttribute].ToString();
                    Sprite image = null;

                    if (reader[cardFileImageAttribute].GetType() == typeof(DBNull))
                    {
                        Debug.Log("Card " + name + " has image set to null.");
                    }
                    else if (images.ContainsKey(imageString) == false)
                    {
                        Debug.LogError(imageString + " is not a image");   
                    }
                    else
                    {
                        image = images[imageString];
                    }

                    int manaCost = reader.GetInt16(8);

                    CardImpact cardImpact = null;

                    if (reader[cardMoveUnitForeignKeyColumnName].GetType() != typeof(DBNull))
                    {
                        int moveDistance = reader.GetInt16(10);
                        cardImpact = new CardMoveUnitImpact(moveDistance);
                    }
                    if (reader[cardCreateUnitForeignKeyColumnName].GetType() != typeof(DBNull))
                    {
                        int health = reader.GetInt16(12);
                        int attack = reader.GetInt16(13);
                        UnitWorld.Unit unitCardCreates = new UnitWorld.Unit(health, attack);
                        cardImpact = new CardCreateUnitImpact(unitCardCreates);
                    }

                    //Change database to start from zero?
                    cards[id - 1] = new Card(name, description, image, cardImpact, manaCost);
                }

                reader.Close();
            }

            populateCardArray();
            connection.Close();
            cardManager.recieveCards(cards);
        }
    }
}