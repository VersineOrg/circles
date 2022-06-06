using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using VersineResponse;
using MongoDB.Bson;
using VersineUser;
using System.Net;

namespace circles;

class HttpServer
{
    public static HttpListener? listener;

    public static async Task HandleIncomingConnections(EasyMango.EasyMango userDatabase,
        EasyMango.EasyMango circleDatabase, WebToken.WebToken jwt)
    {
        while (true)
        {
            HttpListenerContext ctx = await listener?.GetContextAsync()!;

            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;

            Console.WriteLine(req.HttpMethod);
            Console.WriteLine(req.Url?.ToString());
            Console.WriteLine(req.UserHostName);
            Console.WriteLine(req.UserAgent);
            
            if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/userCircles")
            {
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body;
                try
                {
                    body = JsonConvert.DeserializeObject(bodyString)!;
                }
                catch
                {
                    Response.Fail(resp, "bad request");
                    resp.Close();
                    continue;
                }

                string token;
                try
                {
                    token = ((string) body.token).Trim();
                }
                catch
                {
                    token = "";
                }

                if (!String.IsNullOrEmpty(token))
                {
                    string id = jwt.GetIdFromToken(token);
                    if (id == "")
                    {
                        Response.Fail(resp, "invalid token");
                    }
                    else
                    {
                        BsonObjectId userId = new BsonObjectId(new ObjectId(id));
                        
                        if (userDatabase.GetSingleDatabaseEntry("_id", userId,
                                out BsonDocument userBsonDocument))
                        {
                            if (circleDatabase.GetMultipleDatabaseEntries("owner", userId,
                                    EasyMango.EasyMango.SortingOrder.Descending, "name", out List<BsonDocument> bsonCircles))
                            {
                                List<Circle> circles = new List<Circle>();
                                foreach (BsonDocument bsonCircle in bsonCircles)
                                {
                                    circles.Add(new Circle(bsonCircle));
                                }
                                JObject returnMessage = (JObject)JToken.FromObject(circles);
                                
                                Response.Success(resp,"circles found",returnMessage.ToString());
                            }
                            else
                            {
                                Response.Success(resp,"no circles found","");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"user deleted");
                        }
                    }
                }
                else
                {
                    Response.Fail(resp,"invalid body");
                }
            }

            else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/CreateCircle")
            {
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body;
                try
                {
                    body = JsonConvert.DeserializeObject(bodyString)!;
                }
                catch
                {
                    Response.Fail(resp, "bad request");
                    resp.Close();
                    continue;
                }

                string token;
                string friend_id;
                string circleName;
                try
                {
                    token = ((string) body.token).Trim();
                    friend_id = ((string) body.friendId).Trim();
                    circleName = ((string) body.circleName).Trim();
                }
                catch
                {
                    token = "";
                    friend_id = "";
                    circleName = "";
                }

                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(friend_id)))
                {
                    string id = jwt.GetIdFromToken(token);
                    if (id=="")
                    {
                        Response.Fail(resp, "invalid token");
                    }
                    else
                    {
                        BsonObjectId userId = new BsonObjectId(new ObjectId(id));
                        BsonObjectId friendId = new BsonObjectId(new ObjectId(friend_id));

                        if (userDatabase.GetSingleDatabaseEntry("_id", userId,
                                out BsonDocument userBsonDocument))
                        {
                            User user = new User(userBsonDocument);
                            if (user.friends.Contains(friendId))
                            {
                                bool circleExist = false;
                                if (circleDatabase.GetMultipleDatabaseEntries("owner", userId,
                                        EasyMango.EasyMango.SortingOrder.Descending, "name",
                                        out List<BsonDocument> bsonCircles))
                                {
                                    foreach (BsonDocument bsonCircle in bsonCircles)
                                    {
                                        Circle circle = new Circle(bsonCircle);
                                        if (circle.name == circleName)
                                        {
                                            circleExist = true;
                                        }
                                    }
                                }
                                if (circleExist)
                                {
                                    Response.Fail(resp,"circle already exists");
                                }
                                else
                                {
                                    Circle circle = new Circle(userId,circleName);
                                    circle.users.Add(friendId);
                                    if (circleDatabase.AddSingleDatabaseEntry(circle.ToBson()))
                                    {
                                        Response.Success(resp,"created circle","");
                                    }
                                    else
                                    {
                                        Response.Fail(resp,"an error occured, please try again later");
                                    }
                                }
                            }
                            else
                            {
                                Response.Fail(resp,"user provided isn't a friend");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"user deleted");
                        }
                    }
                }
            }
            else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/addToCircle")
            {
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body;
                try
                {
                    body = JsonConvert.DeserializeObject(bodyString)!;
                }
                catch
                {
                    Response.Fail(resp, "bad request");
                    resp.Close();
                    continue;
                }

                string token;
                string friend_id;
                string circleName;
                try
                {
                    token = ((string) body.token).Trim();
                    friend_id = ((string) body.friendId).Trim();
                    circleName = ((string) body.circleName).Trim();
                }
                catch
                {
                    token = "";
                    friend_id = "";
                    circleName = "";
                }

                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(friend_id)))
                {
                    string id = jwt.GetIdFromToken(token);
                    if (id=="")
                    {
                        Response.Fail(resp, "invalid token");
                    }
                    else
                    {
                        BsonObjectId userId = new BsonObjectId(new ObjectId(id));
                        BsonObjectId friendId = new BsonObjectId(new ObjectId(friend_id));

                        if (userDatabase.GetSingleDatabaseEntry("_id", userId,
                                out BsonDocument userBsonDocument))
                        {
                            User user = new User(userBsonDocument);
                            if (user.friends.Contains(friendId))
                            {
                                bool circleExist = false;
                                if (circleDatabase.GetMultipleDatabaseEntries("owner", userId,
                                        EasyMango.EasyMango.SortingOrder.Descending, "name",
                                        out List<BsonDocument> bsonCircles))
                                {
                                    List<Circle> circles = new List<Circle>();
                                    foreach (BsonDocument bsonCircle in bsonCircles)
                                    {
                                        Circle circle = new Circle(bsonCircle);
                                        if (circle.name == circleName)
                                        {
                                            circleExist = true;
                                            circle.users.Add(friendId);
                                            
                                            if (circleDatabase.ReplaceSingleDatabaseEntry("_id",
                                                    bsonCircle.GetElement("_id").Value.AsObjectId,
                                                    circle.ToBson()))
                                            {
                                                Response.Success(resp,"added user to circle circle","");
                                            }
                                            else
                                            {
                                                Response.Fail(resp,"an error occured, please try again later");
                                            }
                                        }
                                    }
                                }
                                if (!circleExist)
                                {
                                    Response.Fail(resp,"Circle has not been created");
                                }
                            }
                            else
                            {
                                Response.Fail(resp,"user provided isn't a friend");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"user deleted");
                        }
                    }
                }
            }
            else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/removeFromCircle")
            {
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body;
                try
                {
                    body = JsonConvert.DeserializeObject(bodyString)!;
                }
                catch
                {
                    Response.Fail(resp, "bad request");
                    resp.Close();
                    continue;
                }

                string token;
                string friend_id;
                string circleName;
                try
                {
                    token = ((string) body.token).Trim();
                    friend_id = ((string) body.friendId).Trim();
                    circleName = ((string) body.circleName).Trim();
                }
                catch
                {
                    token = "";
                    friend_id = "";
                    circleName = "";
                }

                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(friend_id)))
                {
                    string id = jwt.GetIdFromToken(token);
                    
                    BsonObjectId userId = new BsonObjectId(new ObjectId(id));
                    BsonObjectId friendId = new BsonObjectId(new ObjectId(friend_id));

                    if (circleDatabase.GetMultipleDatabaseEntries("owner", userId, out List<BsonDocument> bsonCircles))
                    {
                        bool userIsInCircle = false;
                        foreach (BsonDocument bsonCircle in bsonCircles)
                        {
                            Circle circle = new Circle(bsonCircle);
                            
                            if (circle.users.Contains(friendId) && circle.name == circleName)
                            {
                                userIsInCircle = true;
                                circle.users.Remove(friendId);
                                if (circle.users.Count > 0)
                                {
                                    if (circleDatabase.ReplaceSingleDatabaseEntry("_id",
                                            bsonCircle.GetElement("_id").Value.AsObjectId,
                                            circle.ToBson()))
                                    {
                                        Response.Success(resp,"circle deleted","");
                                    }
                                    else
                                    {
                                        Response.Fail(resp,"an error occured, please try again later");
                                    }
                                }
                                else
                                {
                                    if (circleDatabase.RemoveSingleDatabaseEntry("_id",
                                            bsonCircle.GetElement("_id").Value.AsObjectId))
                                    {
                                        Response.Success(resp,"circle deleted","");
                                    }
                                    else
                                    {
                                        Response.Fail(resp,"an error occured, please try again later");
                                    }
                                }
                            }
                        }

                        if (!userIsInCircle)
                        {
                            
                        }
                    }
                    else
                    {
                        Response.Fail(resp,"circle doesn't exist");
                    }
                }
            }
            else if (req.HttpMethod == "GET" && req.Url?.AbsolutePath == "/health")
            {
                Response.Success(resp,"service up","");
            }
            else
            {
                Response.Fail(resp, "404");
            }
            // close response
            resp.Close();
        }
    }

    public static void Main(string[] args)
    {
        IConfigurationRoot config =
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();
        
        // Get values from config file
        string connectionString = config.GetValue<String>("connectionString");
        string userDatabaseNAme = config.GetValue<String>("userDatabaseNAme");
        string userCollectionName = config.GetValue<String>("userCollectionName");
        string circleDatabaseNAme = config.GetValue<String>("circleDatabaseNAme");
        string circleCollectionName = config.GetValue<String>("circleCollectionName");
        string secretKey = config.GetValue<String>("secretKey");
        uint expireDelay = config.GetValue<uint>("expireDelay");

        // Json web token
        WebToken.WebToken jwt = new WebToken.WebToken(secretKey,expireDelay);
        
        // Create EasyMango databases
        EasyMango.EasyMango userDatabase = new EasyMango.EasyMango(connectionString,userDatabaseNAme,userCollectionName);
        EasyMango.EasyMango circleDatabase = new EasyMango.EasyMango(connectionString,circleDatabaseNAme,circleCollectionName);

        
        // Create a Http server and start listening for incoming connections
        string url = "http://*:" + config.GetValue<String>("Port") + "/";
        listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        Console.WriteLine("Listening for connections on {0}", url);

        // Handle requests
        Task listenTask = HandleIncomingConnections(userDatabase,circleDatabase, jwt);
        listenTask.GetAwaiter().GetResult();
        
        // Close the listener
        listener.Close();
    }
}
