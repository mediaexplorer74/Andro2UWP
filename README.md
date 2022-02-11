# Andro2UWP

## Abstract

My attempt to recover PKar70's Andro2UWP project, branch 10.2002.5

Technical version (build) 1.0.11.0

Status: alpha (RnD)

## Progress 
- Merging Goolge and UWP branches' one drive mechanisms - ready
- RU Localization - added (and symply tested, not ready yet)
- Android version improved (Sync Engine recovered a partially, zero-size files generated, heh)
- UWP version demaged after choosing ARM building... so strange  :(


## Idea (Goal)
New solution for whatsapp-telegram-bridge via Android-phone/Win10Mobile phone "interconnect" 

The beginning starts at this discussion: https://forum.xda-developers.com/t/new-solution-for-whatsapp-telegram-bridge.4211421/

## Progress
Phases of "building the solution": 

[1] RnD +-

[2] Design +-

[3] Tech. project +-

[4] Dev >> [35%]

[5] "Intro" -


## Screenshots
![Shot 1](Images/shot1.png)
![Shot 2](Images/shot2.png)

## Description
### Android app
This app takes your Android notifications, and some of them (filtered using rules you set) 
send to your OneDrive (folder: Apps/Andro2UWP). You can view it either as simple text files 
(from any browser, from any device) or using UWP companion app.

It doesn't use any other data, and it doesn't send anything else anywhere else.

### UWP app
This app reads files from your OneDrive (folder: Apps/Andro2UWP), treating it as Android notification sent by Android companion app.

It doesn't use any other data, and only data it send (to same OneDrive folder) are dictionary of renames of notification 
sender names and notification filters, both created by you.

## Requirenments
- Win SDK (min os build): 15063
- Targets: ARM; x86; x64

## My "Workbench" 

Visual Studio 2022

Used Workloads:
- Xamarin
- UNO Platform extesion


## Refactoring tips:
- UWP/Android/Shared nodes are slitly fixed...
- Onedrive SDK for UWP is ok, but Onedrive SDK for Android is not stable yet...
- Real toast notifications constructed only at/in draft... 

# Contribute!
There's still a TON of things missing from this proof-of-concept (MVP) and areas of improvement 
which I just haven't had the time to get to yet.
- UI Improvements (for GTK, for example, or for each one of supported mutli-platforms)))
- Additional Language Packages
- Media Transferring Support: screenshots, etc. (for the brave)

## Solution Layout
Projects have a DOCs which expands on the internal functionality and layout of that project... it's good for your own R.E. 
There are no DOCs which explains all architecture, API, OneDride auth. and etc. yet... this is just beginning... =)

With best wishes,

  [m][e] 2022


## Thanks!
I wanted to put down some thank you's here for folks/projects/websites that were invaluable for helping me get this project into a functional state:
- [Piotr Karocki](https://github.com/pkar70/) - Great C#/Xamarin/UNO Platform developer
- [Andro2UWP](https://github.com/pkar70/Andro2UWP) - Original Andro2UWP


## License & Copyright

Andro2UWP 1 is RnD project only. AS-IS. No support. Distributed under the MIT License.  

