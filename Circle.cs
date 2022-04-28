using MongoDB.Bson;

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
}