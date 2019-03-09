# FileSharper

[![FileSharper](https://raw.githubusercontent.com/adv12/FileSharper/master/docs/images/screenshotSmall.png)](https://raw.githubusercontent.com/adv12/FileSharper/master/docs/images/screenshotSmall.png)


FileSharper is a C#/WPF file search and processing application for developers and power users.  It allows you to search for files matching certain conditions (length, content, etc.) and take action on them, for instance replacing certain text content or adding a file to a zip archive.  It provides a GUI alternative to command-line workflows based on UNIX tools like `find`, `grep`, `sed`, `sort`, etc.  Search conditions currently include:

* Compound
  * All
  * Any
* Binary
  * Binary Data Contains Byte Sequence
  * Binary Data Contains Text
* Filesystem
  * File Age
  * File Date
  * File Length
  * File Path Contains Text
  * Read-Only
* Image
  * Image Aspect Ratio
  * Image Size
* Text
  * Contains Text
  * Line Count
  * Line Endings
  * Text Encoding Matches
  * Word Count
* XML
  * XPath Node Count
  * XPath Result String Value Contains Text

File Processors currently include:

* Report
  * Write to CSV
* Compound
  * Collect output files
* Filesystem
  * Copy file
  * Create directory
  * Create or update file
  * Move to Recycle Bin
  * Set file date
* Image
  * Create resized image
* Miscellaneous
  * Execute command line
  * Zip file(s)
* Shell
  * Open containing folder
  * Open file
* Text
  * Change line endings
  * Convert case
  * Filter lines
  * Prepend or append text
  * Remove repeated lines
  * Replace text
  * Sort lines
  * Spaces to tabs
  * Tabs to spaces
  * Trim whitespace
  
For a demo, see [this video](https://youtu.be/fp4KZXTEaZE) or [this video](https://youtu.be/ACOryNATedE) on YouTube.  To try it out yourself, grab a possibly-somewhat-recent installer [here](http://www.flamingtortoise.com/FileSharper/Download).  For a tutorial of how to use FileSharper, see [Using FileSharper](https://github.com/adv12/FileSharper/wiki/Using-FileSharper).

FileSharper is designed to be quickly extended with new functionality.  It's published under the MIT license so you can grab the source and add your own domain-specific code.  The GUI is data-driven, so you don't have to write any UI code to add a new search condition or file processor.

You can extend FileSharper's functionality by writing classes that implement any of the four main interfaces:

1. **IFileSource** - an object capable of producing an **IEnumerable&lt;FileInfo&gt;** that acts as the source of filenames that will be tested and processed by the application.
2. **ICondition** - an object capable of testing whether a certain condition is true or false for a given file.
3. **IFieldSource** - an object capable of producing one or more pieces of information about a file.
4. **IProcessor** - an object that takes some action given an input file.

When you add a class implementing one of these interfaces to the FileSharperCore project or a project of your own that you add to the FileSharper solution, it will be found at runtime via reflection and added to the appropriate picklist in the FileSharper UI.  The FileSharper UI is used to create "searches" that have exactly one file source, zero or one condition (which can be an arbitrary composition of other conditions), any number of field sources, and any number of processors that run when a file is tested or matched.

If you're adding file sources, conditions, field sources, or processors, I recommend inheriting from the base classes **FileSourceBase**, **ConditionBase**, **FieldSourceBase**, and **SingleFileProcessorBase** (or **ProcessorBase** if you know what you're doing).

If you download FileSharper and start adding classes that would benefit others, consider submitting pull requests so I can pull them in and make FileSharper better for everyone.  Right now the code is marked as (c) Andrew Vardeman, but if I actually get contributions I'll adopt a contributor's license agreement so it can become more of a community project.

Caveat Emptor: FileSharper is currently roughly alpha quality.  I'm slowly adding tests, starting with the various Processors, as they run the greatest risk of damaging your files.

Caveat #2: FileSharper's handling of text encodings other than ASCII/UTF-8 is largely untested.  I'd recommend caution when running processors that overwrite the source text file.  You will probably want to do a test run with a file you've backed up before trusting it to do the right thing.
