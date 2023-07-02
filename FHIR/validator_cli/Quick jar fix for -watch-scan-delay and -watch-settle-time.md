A single-letter [typo in `Params.java`][typo] currently prevents the use of the `-watch-scan-delay` and `-watch-settle-time` validator command-line parameters. A permanent fix is [already in the pipeline][PR]. Until an updated build becomes available, the problem can easily be fixed by replacing a single class file in the jar, without having to set up a build environment for the whole shebang.

The idea is simple: start a new project in your favourite Java IDE and add `validator_cli.jar` as a library. Download the `Params.java` source file from GitHub (or use the decompiled version producted by IntelliJ), add it to the project, and fix the following line in `Params::readInteger` (line 434 in the original source file):

```
-    return Integer.parseInt(n);
+    return Integer.parseInt(v);
```

Compile (Ctrl+F9) and substitute the class file in the jar. Note: the jar can easily be opened like any other ZIP file with an unorthodox file extension; for example, by selecting it in Total Commander and hitting Ctrl+PgDn.

[typo]: https://github.com/hapifhir/org.hl7.fhir.core/blob/6db701767ac1476411fa1ac2e3814cc3e3e3b667/org.hl7.fhir.validation/src/main/java/org/hl7/fhir/validation/cli/utils/Params.java#L434-L435
[PR]: https://github.com/hapifhir/org.hl7.fhir.core/pull/1333