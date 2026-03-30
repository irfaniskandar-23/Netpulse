# Context

These are my understanding as of now of what Global Exception handler does and some of my inner thoughts and assumptions that requires guidance and knowledge

## what it does (I think so 😅)

- Capture **unhandled** exception or explicitly thrown exception in the entire project.
- Determines HTTP Status code to set for the response
- Create a generic error respons wrapper and default to normal message as follows

```json
{ "message": "internal server error, please try again later!" }
```

## my assumptions (do not grill me 😅)
- Need to use `problem details` as a standard, since it is popular and suggest by many popular developer
- API need to convey the message clearly yo end user what went wrong instead of generic message, like database down, missing configurations
- i always feel like to add something to the response header here but not sure what is it hahaha,
- Use result pattern for business logic failures to convey message to users
- avoid throwing exception for business domain logic/rules, exception is expensive (not sure why it is expensive though)


## What I Need to know more
- Cancellation token usage in exception handler, does it handle memory leaks, disconnected TCP conncetion by client / request timeout
- what more can you suggest i need to know in global exception handler