# cronus
This project is a midleware of ESL Gen 3.0 solution.

Note: Before you to put this middleware into your project, you must check both of your labels and aps are the 3rd generation from ETAG.

The Cronus/Cronus folder is the middleware source code.
The Cronus/Cronus.API folder is the API package source code for cross language platform to intergration (TODO).
The Cronus/Cronus.Demo folder is a WPF desktop demo to using Cronus/Cronus.

## Quick start:
0. Add project reference;
1. Register AP status changed event and Task feedback event:
```SendServer.Instance.APEventHandler += Instance_APEventHandler;```
```SendServer.Instance.TaskEventHandler += Instance_TaskEventHandler;```
2. Start the Sendserver:
```var result = SendServer.Instance.Start(_config, _logger);```
3. Send a image to the ESL:
```var result = SendServer.Instance.Send(tagID, image);```

For more information, please read the D19 documet.
Some sections are still under construct, for issues and bugs please let me know, thanks.
