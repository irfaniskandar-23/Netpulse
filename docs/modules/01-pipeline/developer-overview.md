# Context

this is my current overview and understanding of .NET core api request pipeline followed with some confusion that i have
after reading this file and using the explaining-cocepts skill, hopefully can help firming my understanding

## Purpose

this is what i understand as of now

- Act as a gatekeep before request hit the API controller.
- Everything that needed to be done at all controller, can be moved up to a specific middleware (same code one place,avoid duplicated across controllers).
- Focuses more on the http request response action (logging,exception handling,authentication)
- Help request to short circuit early when some conditions are not met in the middleware (example: missing/invalid auth)

## Question

- Unclear on wiring up the middleware in `program.cs`, Been seing other developers using extension method with services
- confused on the order of middleware, does requst response logging middleware comes first or exception middleware comes first?
- if exception middleware comes first, then how the request response middleware will log the exception
- what is the different between using exception handler page in development environment and production
- alwauys confused on the default program.cs provided by .NET core api template, seems it alreay have useauthentication and authorization middleware. does this mean we cannot or should not use our own auth middleware? what is provided internally by the .NET built in authentication and authorization
- what is use https redirection in the program.cs setup
- what we should and should not do with HttpContext object in .NET
