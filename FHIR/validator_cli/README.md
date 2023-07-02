`validator_cli.jar` is the command line version of the [hl7.fhir.core][hfc] validator. The GitHub repository lists [all releases][rel], and the latest version is always available under the same [download link][lnk] (ETag supported). The official documentation is at the [FHIR Validator Confluence page][doc].


[hfc]: https://github.com/hapifhir/org.hl7.fhir.core
[rel]: https://github.com/hapifhir/org.hl7.fhir.core/releases
[lnk]: https://github.com/hapifhir/org.hl7.fhir.core/releases/latest/download/validator_cli.jar
[doc]: https://confluence.hl7.org/display/FHIR/Using+the+FHIR+Validator

# Watch Mode

Since version 6.0.16 the validator has a so-called watch mode, in which it watches the filesystem for changes and re-validates any changed inputs (`-watch-mode single`) or all inputs (`-watch-mode all`) whenever any of the inputs changes.

The original version scanned the file system for changes every 1000 ms, but since version 6.0.17 the delay can be configured via command line parameters. 

`-watch-scan-delay` specifies how often - in milliseconds - the validator scans the filesystem for changes. `-watch-settle-time` specifies how many milliseconds the validator waits for the dust to settle after detecting a change, in order to mitigate issues with files disappearing for brief moments.

Unfortunately, a single-letter typo in `Params.java` currently (version 6.0.20) makes it necessary to substitute `Params.class` in the jar file in order to enable the use of the `-watch-scan-delay` and `-watch-settle-time` command-line parameters. Until an updated build becomes available, there is an [easy fix/workaround][fix].

[fix]: ./Quick%20jar%20fix%20for%20-watch-scan-delay%20and%20-watch-settle-time.md

# Using `validator_cli` in Watch Mode as a Server

There are many interesting uses of watch mode, like running a watch mode validator in a VS Code terminal window in order to get whatever you're editing revalidated every time you hit Ctrl+S to save.

Perhaps not quite so obvious - but no less useful - is that fact that a watch mode validator is effectively a validation server, one with which you communicate via the file system (i.e. 'poor mans's IPC') instead of HTTP or domain sockets.

Have the validator look for changes in an input file with a name that reflects the validator configuration (FHIR version, loaded packages/IGs etc.) and have it write an output file whose name is correlated to the name of the input file. Then you can have several running validator instances with different configurations, and you can address any one of them directly via the name of its suitable chosen input and output filenames.

As an example, in order to validate resources from the subject area of electronic prescriptions in Teutonia I need a certain set of profile packages for resources from last Friday (June 30th) or older, and a certain other set of packages for resources dating from Saturday (July 1st) or newer. So, using 'ERezept2023Q2' and 'ERezept2023Q3' as configuration names, I have one validator watch `ERezept2023Q2-input.xml` and write `ERezept2023Q2-output.eslint-compact`, and another one watches `ERezept2023Q3-input.xml` and writes `ERezept2023Q3-output.eslint-compact`. 

This is exactly the manner in which `validator_cli` is slaving away in production at our shop, double-checking on the results of the [ABDA/DAV reference validator][ref] (around which I had wrapped an HTTP API eons ago, but that was a project in its own right and a whole lot more work than simply setting up a watch mode `validator_cli`).

[ref]: https://github.com/DAV-ABDA/eRezept-Referenzvalidator

## [xp_WatchModeClient.linq][linq]

The LINQPad script [xp_WatchModeClient.linq][linq] (C#) contains the necessary logic wrapped in a simple class called `WatchModeClient` plus some demo code for exercising the class. 

Pre-configured experiments (besides '1st steps') include validating an empty Patient resource in a tight loop in order to gauge the throughput of the communication method, and validating all the resources in a FHIR core package (by reaching directly into the machine-local package cache).

The simple watch mode client does not configure and start the validators (although it can tell you the necessary command line). At the moment I use a simple shell script ([$watch_mode_server.cmd][cmd]) for that. This script uses another script - `$validator_cli.cmd` - for selecting/locating a version `validator_cli.jar` (specified version or newest) and setting certain personal or machine-dependent default parameters (proxy or no proxy, FHIR version R4) and so on. Simple replace it with the moral equivalent of

```
java -jar validator_cli.jar %*
```

A more complete client would be able to select and locate a JRE, select and configure a validator_cli version, run the whole shebang while having console output piped to a convenient place, and it would also manage mutual exclusion (i.e. prevent more than one validator being started for the same configuration or input/output filenames). 'Configuring' a validator would also include determining the set of packages to load for a certain validation goal, or figuring out how many different, mutually incompatible package sets - and hence necessary validator instances - result from a certain set of validation goals and route requests to stable full of validators it created. However, stuff like that goes far beyond what's feasible for a simple demo, and it would only distract from the essentials. 

[linq]: ./xp_WatchModeClient.linq
[cmd]: ./%24watch_mode_server.cmd