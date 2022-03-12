# Mod Rewriter
A .NET 6 library and console application for programmatically rewriting [_tModLoader_](https://github.com/tModLoader/tModLoader/) modifications. Useful for porting to and from the latest alpha 1.4 version.

A rewriter handler instance supports installing rewriters, which may process code and make changes as deemed fit.

This project takes heavy inspiration from Chik3r's _[tModProter](https://github.com/Chik3r/tModPorter)_, and the core rewriting system is directly adapted from its source code. Why not leave it a star?


# Project Structure
_Mod Rewriter_ is divided into serveral different projects, designed for different uses:

* `ModRewriter.Core` - The core inner-workings. Contains the bare minimum: a rewriting framework.
* `ModRewriter.Rewriters` - A collection of rewriters used in a _tModLoader_-specific context.
* `ModRewriter.Console` - A basic console application for managing _tModLoader_ mod rewriting using rewriters from `ModRewriter.Rewriters`.