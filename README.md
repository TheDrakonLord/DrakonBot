Title: Havoc Bot <br />
Author: Neal Jamieson<br />
Version: 0.3.0.0<br />
 <br />
Description:<br />
    A discord bot aimed at combining the functions of serveral other bots into one<br />
    currently the bot handles the functionality of: <br />
        -scheduling and announcing events.<br />
        -Retrieving and storing FFXIV character data
    This program uses xml to store its data with the exception of the log which is stored as a text file.<br />
    This bot does not currently support multiple guilds.<br />
    <br />
Dependencies:<br />
    botEvents.cs<br />
    CommandHandler.cs<br />
    globals.cs<br />
    HavocBot.cs<br />
    InfoModule.cs<br />
    Program.cs<br />
    <br />
Data Files:<br />
    commandData.xml<br />
    botLog.txt<br />
    <br />
Dependent on:<br />
    xml.linq library<br />
    Discord.net<br />
    <br />
References:<br />
    xml.linq: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/<br />
    xml: https://www.w3schools.com/xml/xml_elements.asp<br />
    This code is adapted from the instructions and samples provided by the Discord.net Documentation found at:<br />
    https://docs.stillu.cc/guides/introduction/intro.html<br />
