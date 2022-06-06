using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace circles;

public class Circle
{
    public BsonObjectId owner;
    public String name;
    public List<BsonObjectId> users;
    
    public Circle(BsonObjectId owner, string name)
    {
        this.owner = owner;
        this.name = name;
        users = new List<BsonObjectId>();
    }
    
    public Circle(BsonDocument document)
    {
        owner = document.GetElement("owner").Value.AsObjectId;
        name = document.GetElement("name").Value.AsString;
        users = new List<BsonObjectId>();
        BsonArray? usersArray = document.GetElement("users").Value.AsBsonArray;
        foreach (var userBson in usersArray)
        {
            users.Add(userBson.AsObjectId);
        }
    }
    
    public BsonDocument ToBson()
    {
        return new BsonDocument(
            new BsonElement("owner",owner),
            new BsonElement("name",name),
            new BsonElement("users",new BsonArray(users)));
    }
    
    public static string CircleToJson (List<BsonDocument> listCircle)
    {
        string circleListJson = "[";
        int l = listCircle.Count;
        for (int i = 0; i < l; i++)
        {
            Circle circle = new Circle(listCircle[i]);
            string circleJson = JsonConvert.SerializeObject(circle);
            circleListJson += circleJson;
            if (i < l - 1)
            {
                circleListJson += ", ";
            }
        }
        circleListJson += "]";
        return circleListJson;
    }
}