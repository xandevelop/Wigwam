# Wigwam
A user friendly way to write Selenium tests - more powerful than record/replay, less code than Cucumber for the QA team.

Currently a WIP - version 0.1.
Todo for v1.0 to be considered complete:
- Finish core implementation, including tricky stuff like function signatures
- Add a load of commands to the core implementation (there's a distinction between core "how does it basically work" vs "make it powerful with lots of commands that rely on similar infrastructure")
- Add documentation on how to write tests (already written in design phase, but not uploading yet as it's not all implemented - if you tried to follow the docs now it wouldn't work)
- Write exporter tools.  Tests we generate aren't executable without some sort of runner tool.  There are loads of options here, so plan is to support a few and write up how to create new ones.  Typically this will be a simple text transform sort of difficulty level.

For v2.0 to be considered complete:
- IDE for scripts, to make them even easier to write
- Demo builder software with this as a built in scripting language (will be in separate repo that uses this tech)
