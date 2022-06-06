using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        return new BsonDocument((IEnumerable<BsonElement>)
            new BsonElement[] 
            { 
            new ("owner",owner),
            new ("name",name),
            new ("users",new BsonArray(users))
            });
    }
    
    public static string CircleToJson (List<BsonDocument> listCircle)
    {
        string circleListJson = "[";
        int l = listCircle.Count;
        for (int i = 0; i < l; i++)
        {
            BsonDocument circleBson = listCircle[i];
            Circle circle = new Circle(circleBson);
            
            JObject circleJson = JObject.FromObject(circle);
            circleJson.Property("owner").AddBeforeSelf(
                new JProperty("id", circleBson.GetElement("_id").Value.AsObjectId.ToString()));
            circleListJson += circleJson.ToString();
            
            
            if (i < l - 1)
            {
                circleListJson += ", ";
            }
        }
        circleListJson += "]";
        return circleListJson;
    }
}