# door

## role

the circles microservice manages circles

## usage

the circle microservice has the following endpoints:

### list circles of a user (/userCircles)

##### method

GET

##### body

{
  "token":"put_token_here",
}

##### response

// TODO : write documentation for this part

### add a friend to a circle (/addToCirlce)

##### method

POST

##### usage

this endpoint adds a user to a circle
or creates a new circles if the specified
circle name doesn't exist

##### body

{
  "token":"put_token_here",
  "friendId":"put_friendId_here",
  "circleName":"put_circle_name_here"
}

### remove a friend from a circle (/removeFromCircle)

##### method

POST

##### usage

this endpoint removes a user from a circle
and deletes the circle if it's empty

##### body

{
  "token":"put_token_here",
  "friendId":"put_friendId_here",
  "circleName":"put_circle_name_here"
}

## configuration

this microservice is configured with the file appsettings.json.
a template file is provided, the expected database is a MongoDB database


// sorry idk how to write a doc but it's still a lot better than our EPITA tp's isn't it ? - Rayan
