Voxalia
-------

**Voxalia, a game about blocks and such.**

Built using OpenTK (a C# binding of OpenGL for the 3D rendering), BEPUphysics (a capable pure-C# physics engine), and FreneticScript (powerful and adaptive game modding).

Also used libraries:

- Open.NAT (automated port forwarding)
- Opus (network-ready voice compression)
- csogg/csvorbis (.ogg audio file handling)
- LiteDB (save file compression)
- YAMLDotNet (used for FreneticScript data handling and some files in Voxalia's save structure)

## Windows Install Notes

- Requires OpenAL (Run `oalinst.exe` from the assets repo)
- Requires reasonably up to date graphics drivers
- Requires a 64-bit system
- Windows build is primarily tested on Windows 10 with latest NVidia drivers.

## Linux Install Notes

- Requires mono.
- Requires LibOpus.
- Requires fully up to date non-Intel graphics drivers (Must support OpenGL 4.3).
- Requires "espeak" program/package. Must be a valid "espeak" executable in path.
- Requires a 64-bit system.
- Linux build is primarily tested on Ubuntu 16.04 with latest NVidia drivers.

## Mac Install Notes

- Should be similar to Linux requirements...
- ??? (Untested!)

## Compiling

- Open the .sln in Microsoft Visual Studio 2015
- Switch the configuration to Release|x64
- Build -> Build Solution!

## Playing

- Compile everything (see above)
- Download the assets (included in public releases, or via the private repository)
- Run `Voxalia.exe` by double clicking it on Windows, or via `$ mono Voxalia.exe` on Linux/Mac.
- Open your console with the `tilde` key (the key next to the 1 key on standard US English QWERTY keyboards)
- Type "`login <username> <password>`" (To get an account, register at http://frenetic.xyz/account/register )
- Close Voxalia after it confirms login
- Double click `singleplayer.bat` (Windows) or `singleplayer.sh` (Linux/Mac) to open that, then wait for it to load
- You should now be in the game!

## Also Included

- ModelToVMDConvertor -> converts any given model format (via AssImp) to VMD, the model format used by Voxalia.
- SkeletalAnimationExtractor -> converts .dae animated models to reusable anim files.
- VoxaliaServerSamplePlugin -> a sample of a C# powered plugin for the Voxalia server.

## FAQ

- **Where do I get Voxalia assets?**
	- At the current time, Voxalia assets are being kept private. If you wish to playtest Voxalia, join us on IRC at irc.frenetic.xyz#voxalia - https://client01.chat.mibbit.com/?channel=%23voxalia&server=irc.frenetic.xyz
- **Where can I learn more about Voxalia?**
	- At the current time, information is largely restricted. If you wish to learn more, join us on IRC (see above question).
- **What do I need to play Voxalia over LAN?**
	- Run Voxalia server as normal, and disable CVar `n_verifyip` by executing server command: `set n_verifyip false`.
- **Do I need to run the Voxalia server to play with friends?**
	- No. Your friends can join your singleplayer game as well. (There may be additional steps in the future to open a singleplayer game fully.)
- **What port do I need to forward?**
	- By default Voxalia server and singleplayer open up on port 28010. You can edit this in your launch command options (first argument is always the port).
	- Note that port forwarding is partially automated by the internals and it generally isn't required that you manually forward it.

### Licensing pre-note:

This is an open source project, provided entirely freely, for everyone to use and contribute to.

If you make any changes that could benefit the community as a whole, please contribute upstream.

### The short of the license is:

You can do basically whatever you want, except you may not hold any developer liable for what you do with the software.

### The long version of the license follows:

The MIT License (MIT)

Copyright (c) 2016 FreneticXYZ

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

----

----

![YourKit](https://www.yourkit.com/images/yklogo.png)

The FreneticXYZ team uses YourKit .NET Profiler to improve performance. We'd like to thank them for their amazing tool and recommend them to all .NET developers!

YourKit supports open source projects with its full-featured .NET Profiler.  
YourKit, LLC is the creator of [YourKit .NET Profiler](https://www.yourkit.com/.net/profiler/index.jsp)  
and [YourKit Java Profiler](https://www.yourkit.com/java/profiler/index.jsp)  
innovative and intelligent tools for profiling .NET and Java applications.
