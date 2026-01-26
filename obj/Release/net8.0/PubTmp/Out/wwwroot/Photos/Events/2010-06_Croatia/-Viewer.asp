<%
Const PRESENTATION_TITLE = "Presentation" 'Used in the title of every page along with a page counter.
Const HOME_PAGE = "./" 'Where you go when you hit the "home" nav button. Can be URL.
Const END_PAGE = "./" 'If you hit "back" when you're at beginning or hit "next" at the end.
Const IMAGE_ALIGN = "right" 'Should be "right" or "left" so image and text can coexist on page.
Const TEXT_EXTENSIONS = "txt" 'Text files to be incorporated into a slide
Const PICTURE_EXTENSIONS = "bmp, jpg, jpeg, gif, png" 'Pictures to be made into a slide
Const HTM_EXTENSIONS = "htm" 'Handled as pictures. You must NOT include "asp" to prevent recursion!
Const SOUND_EXTENSIONS = "mid, wma, au, wav, mp3" 'Sounds (if any) to be played with a slide
Const BACK_BUTTON = "&#9668" 'Text or graphic used for the back navigation button
Const HOME_BUTTON = "&#9650" 'Text or graphic used for the home navigation button
Const NEXT_BUTTON = "&#9658" 'Text or graphic used for the next navigation button
%>

<p align="left">

<font size="2"><a target="_self" href="http://www.themasseys.net/familypix/">
Back to the main picture 
archive</a></font><a target="_self" href="http://www.themasseys.net/familypix/">
<%

Main

Sub Main

Dim varBaseNames, strHtml, lngSlide, blnValidSlide
	varBaseNames = BaseNames()
	'Bail out with an error messageif we didn't find anything to display
	If UBound(varBaseNames) = 0 Then
		strHtml = "<html><head><title>Error - No Files</title></head>"
		strHtml = strHtml & "<body>Error - There are no text, picture, or sound files in "
		strHtml = strHtml & "the " & urlInThisFolder("""") & " folder. </body></html>"
		Response.Write strHtml
		Exit Sub
	End If
	'See if we have a valid slide number
	blnValidSlide = True
	On Error Resume Next
	If Request.QueryString("slide").Count <> 1 Then blnValidSlide = False
	lngSlide = Request.QueryString("slide")
	If Not IsNumeric(lngSlide) Then blnValidSlide = False
	lngSlide = CLng(lngSlide)
	If lngSlide < LBound(varBaseNames) Then blnValidSlide = False
	If lngSlide > UBound(varBaseNames) Then blnValidSlide = False
	On Error Goto 0
	'Write the appropriate slide
	If blnValidSlide Then
		Response.Write WebPage(lngSlide, varBaseNames)
	Else
		Response.Write WebPage(0, varBaseNames)
	End If
End Sub

Function BaseNames()
'Returns an array of names (no extensions or paths)
Dim varBaseNames(), strBaseName, fs, fol, fil, fils, strExtension, blnZero
	Set fs = CreateObject("Scripting.FileSystemObject")
	Set fol = fs.GetFolder(Server.MapPath("./"))
	Set fils = fol.Files
	ReDim varBaseNames(0)
	blnZero = True
	For Each fil In fils
		For Each strExtension In Split(TEXT_EXTENSIONS & "," & PICTURE_EXTENSIONS & "," & SOUND_EXTENSIONS & "," & HTM_EXTENSIONS, ",")
			strExtension = Trim(strExtension)
			If strExtension <> "" Then
				If Left(strExtension, 1) <> "." Then strExtension = "." & strExtension
				If Lcase(Right(fil.Name, Len(Trim(strExtension))))  = Lcase(Trim(strExtension)) Then
					'Remove the file extension
					strBaseName = fil.Name
					strBaseName = Left(strBaseName, (InStrRev(strBaseName, ".") - 1))
					'Add it to our varBaseNames array
					If Not InArray(varBaseNames, strBaseName) Then
						If Not blnZero Then ReDim Preserve varBaseNames(UBound(varBaseNames) + 1)
						blnZero = False
						varBaseNames(UBound(varBaseNames)) = strBaseName
					End If
				End If
			End If
		Next
	Next
	SortAscending varBaseNames, Chr(0), 0
	BaseNames = varBaseNames
End Function

Function InArray(arrHaystack, strNeedle)
Dim strValue
	For Each strValue In arrHaystack
		If strValue = strNeedle Then
			InArray = True
			Exit Function
		End If
	Next
	InArray = False
End Function

Sub SortAscending(strArray, strSplitCharacter, intSortByElement)
Dim blnChanged 'As Boolean
Dim strBuffer 'As String
Dim intCounter 'As Integer
	blnChanged = True
	Do Until Not blnChanged
		blnChanged = False
		For intCounter = Lbound(strArray) + 1 to Ubound(strArray)
			If Split(strArray(intCounter -1), strSplitCharacter)(intSortByElement) > Split(strArray(intCounter), strSplitCharacter)(intSortByElement) Then
				blnChanged = True
				strBuffer = strArray(intCounter -1)
				strArray(intCounter -1) = strArray(intCounter)
				strArray(intCounter) = strBuffer
			End If
		Next
	Loop
End Sub

Function WebPage(lngSlideIndex, varBaseNames)
' Returns HTML to display the slide in the array element pointed to by the index
	' HTML page header
	strHtml = "<html>" & vbCrLf & "<head><title>"
	strHtml = strHtml & PRESENTATION_TITLE
	strHtml = strHtml & " - Page " & (lngSlideIndex + 1) & " of " & Ubound(varBaseNames) + 1
	strHtml = strHtml & "</title></head>" & vbCrLf & "<body>" & vbCrLf
	strHtml = strHtml & "<table border='0' width='100%'>" & vbCrLf & "<tr>" & vbCrLf
	' Left-side content
	strHtml = strHtml & "<td valign='top' align='center' width='150' "
	strHtml = strHtml & "style=""border-style: solid; border-width: 2px; "
	strHtml = strHtml & "border-color: rgb(255,255,255) rgb(0,0,0) rgb(255,255,255) rgb(255,255,255); """
	strHtml = strHtml & ">" & vbCrLf
	strHtml = strHtml & LeftSideHtml(lngSlideIndex, varBaseNames)
	strHtml = strHtml & "</td>" & vbCrLf
	' Right-side content
	strHtml = strHtml & "<td valign='top' align='left'>" & vbCrLf
	strHtml = strHtml & RightSideHtml(lngSlideIndex, varBaseNames)
	strHtml = strHtml & "</td></tr>" & vbCrLf & "</table>" & vbCrLf
	' Add script to make the "Next" button have focus
	strHtml = strHtml & "<script>setTimeout('document.getElementById(""Next"").focus()', 1000)</script>"
	strHtml = strHtml & "</body>" & vbCrLf & "</html>" & vbCrLf
	WebPage = strHtml
End Function

Function LeftSideHtml(lngSlideIndex, varBaseNames)
Dim strHtml, strSound
	strHtml = ""
	strHtml = strHtml & "<table border='0' height='100%'>" & vbCrLf
	strHtml = strHtml & "<tr><td valign='top' align='center'>" & vbCrLf
	' Navigation button code
	strHtml = strHtml & navigationButtons(lngSlideIndex, varBaseNames)
	strHtml = strHtml & "</td></tr>" & vbCrLf
	' Links code
	strHtml = strHtml & "<tr><td valign='top' align='center'>" & vbCrLf
	strHtml = strHtml & linksList(varBaseNames)
	strHtml = strHtml & "</td></tr>" & vbCrLf
	' Media Player code
	strHtml = strHtml & "<tr><td valign='bottom' align='center'>" & vbCrLf
	strSound = RealFileName(lngSlideIndex, varBaseNames, SOUND_EXTENSIONS)
	If strSound <> "" Then
		strSound = urlInThisFolder(strSound)
		If IsIE() Then
			' The SoundIE function needs to know the URL of the next page
			If lngSlideIndex = Ubound(varBaseNames) Then
				strHtml = strHtml & SoundIE(strSound, END_PAGE)
			Else
				strHtml = strHtml & SoundIE(strSound, ArgumentsForThisUrl("?slide=" & (lngSlideIndex + 1)))
			End If
		ElseIf IsWin() Then
			strHtml = strHtml & SoundWindows(strSound)
		Else
			strHtml = strHtml & SoundGeneric(strSound)
		End If
	End If
	strHtml = strHtml & "</td></tr></table>" & vbCrLf
	LeftSideHtml = strHtml
End Function

Function RightSideHtml(lngSlideIndex, varBaseNames)
'Returns HTML to display the slide
Dim  strHtml
	strHtml = ""
	' Picture
	strHtml = strHtml & PictureHtml(lngSlideIndex, varBaseNames)
	' Text
	strHtml = strHtml & TextHtml(lngSlideIndex, varBaseNames)
	RightSideHtml = strHtml
End Function

Function RealFileName(lngIndex, varBaseNames, strExtensions)
'Returns the name (no path) of a file with one of the extensions. If none, returns empty string.
Dim fs, strExtension, strName
	Set fs = CreateObject("Scripting.FileSystemObject")
	strName = ""
	For Each strExtension In Split(strExtensions, ",")
		strExtension = Trim(strExtension)
		If strExtension <> "" Then
			If Left(strExtension, 1) <> "." Then strExtension = "." & strExtension
			If fs.FileExists(Server.MapPath(varBaseNames(lngIndex) & strExtension)) Then
				strName = varBaseNames(lngIndex) & strExtension
			End If
		End If
	Next
	RealFileName = strName
End Function

Function LinksList(varBaseNames)
Dim lngSlide, strBaseName, blnDirty
	lngSlide = -1 ' Incremented to get the index for each slide in the array
	strHtml = "<font size='-1'>"
	For Each strBaseName In varBaseNames
		lngSlide = lngSlide + 1
		strHtml = strHtml & vbCrLf & "<br><a href='"
		strHtml = strHtml & ArgumentsForThisUrl("?slide=" & lngSlide)
		'Assume there are leading characters we don't want. Remove them from the link text.
		blnDirty = True
		Do While blnDirty
			blnDirty = False
			For Each strChar In Split("0 1 2 3 4 5 6 7 8 9 - _")
				If Left(strBaseName, 1) = strChar Then
					blnDirty = True
					strBaseName = Mid(strBaseName, 2)
					'If the text is all numbers, we'll kill it all and make up a new name.
					If Len(strBaseName) = 1 Then strBaseName = "Page " & lngSlide
				End If
			Next
		Loop
		strBaseName = Trim(strBaseName)
		' Replace underscores with spaces
		strBaseName = Replace(strBaseName, "_", " ")
		strHtml = strHtml & "'>" & strBaseName & "</a>"
	Next
	strHtml = strHtml & vbCrLf & "</font><br>"
	LinksList = strHtml
End Function

Function UrlInThisFolder(strDocument)
'Returns a complete URL pointing to a document in the script's folder
Dim strScript, strPath, strUrl
	strScript = Request.ServerVariables("SCRIPT_NAME")
	strPath = Left(strScript, InStrRev(strScript, "/"))
	strUrl = "http://" & Request.ServerVariables("SERVER_NAME")
	If Request.ServerVariables("SERVER_PORT") <> "80" Then
		strUrl = strUrl & ":" & Request.ServerVariables("SERVER_PORT")
	End If
	strUrl = strUrl & strPath & Replace(strDocument, " ", "%20")
	UrlInThisFolder = strUrl
End Function

Function ArgumentsForThisUrl(strArgs)
' Adds HTTP GET arguments to the existing script. Typical $args = '?foo=bar&afu=yes'
Dim strUrl
	strUrl = "http://" & Request.ServerVariables("SERVER_NAME")
	If Request.ServerVariables("SERVER_PORT") <> "80" Then
		strUrl = strUrl & ":" & Request.ServerVariables("SERVER_PORT")
	End If
	strUrl = strUrl & Request.ServerVariables("SCRIPT_NAME")
	strUrl = strUrl & strArgs
	ArgumentsForThisUrl = strUrl
End Function

Function IsIE()
	If InStr(Request.ServerVariables("HTTP_USER_AGENT"), "MSIE") = 0 Then
		IsIE = False
	Else
		IsIE = True
	End If
End Function

Function IsWin()
	If InStr(Request.ServerVariables("HTTP_USER_AGENT"), "Windows") = 0 Then
		IsWin = False
	Else
		IsWin = True
	End If
End Function

Function PictureHtml(lngSlideIndex, varBaseNames)
' Returns the HTML needed to display the picture or web page (if one exists)
Dim strHtml, strBody, strPicture, blnIsHtml, strExtension, lngPointer
	strHtml = ""
	strPicture = RealFileName(lngSlideIndex, varBaseNames, PICTURE_EXTENSIONS & "," & HTM_EXTENSIONS)
	If strPicture = "" Then
		PictureHtml = strHtml
		Exit Function
	End If
	'Do we have a real picture or a web page?
	blnIsHtml = False
	For Each strExtension In Split(HTM_EXTENSIONS, ",")
		strExtension = Trim(strExtension)
		If strExtension <> "" Then
			If Left(strExtension, 1) <> "." Then strExtension = "." & strExtension
			If Lcase(Right(strPicture, Len(strExtension))) = Lcase(strExtension) Then
				blnIsHtml = True
				Exit For
			End If
		End If
	Next
	If blnIsHtml Then
		'Picture is an htm file! Read it so we can use it's text.
		strBody = File2String(Server.MapPath(strPicture))
		'Use only the text inside the <body> tags
		lngPointer = InStr(1, strBody, "<body", vbTextCompare) 'find the body tag start
		strBody = Mid(strBody, lngPointer) 'remove all before the starting tag
		lngPointer = InStr(1, strBody, ">", vbTextCompare) 'find the close of the start tag
		strBody = Mid(strBody, lngPointer + 1) 'remove all before end of body starting tag
		lngPointer = InStr(1, strBody, "</body", vbTextCompare) 'find the end body tag
		strBody = Left(strBody, lngPointer - 1) 'trim all after the ending tag
		strHtml = strHtml & strBody
	Else
		'Picture is a real graphics file, just insert an img tag
		strHtml = strHtml & "<img src='"
		strHtml = strHtml & urlInThisFolder(strPicture)
		strHtml = strHtml & "'"
		' Only use the align property if we have to mix in some text with the image
		If RealFileName(lngSlideIndex, varBaseNames, TEXT_EXTENSIONS) <> "" Then
			strHtml = strHtml & " align=" & IMAGE_ALIGN
		End If
		strHtml = strHtml & ">" & vbCrLf
		strHtml = strHtml & "<img height=1 width=100><br> " & vbCrLf ' Makes sure there is room for text under Mozilla
	End If
	PictureHtml = strHtml
End Function

Function File2String(strFile)
Dim fs, ts
Const ForReading = 1
	Set fs = CreateObject("Scripting.FileSystemObject")
	If fs.FileExists(strFile) Then
		Set ts = fs.OpenTextFile(strFile, ForReading, True)
		If ts.AtEndOfStream Then
			File2String =""
		Else
			File2String = ts.ReadAll
		End If
		ts.Close
	Else
		File2String = ""
	End If
End Function

Function TextHtml(lngSlideIndex, varBaseNames)
' Returns the HTML needed to display the text data (if text exists)
Dim strHtml, strText
	strHtml = ""
	strText = RealFileName(lngSlideIndex, varBaseNames, TEXT_EXTENSIONS)
	If strText = "" Then
		TextHtml = ""
		Exit Function
	End If
	' Read the text file so we can spit it into our html stream
	strHtml = File2String(Server.MapPath(strText))
	' Escape the text to make it HTML-safe
	strHtml = Server.HTMLEncode(strHtml)
	' Make simple text-html formatting decisions
	strHtml = Replace(strHtml, vbTab, "&nbsp&nbsp&nbsp&nbsp") ' TAB characters
	Do While InStr(strHtml, "  ") <> 0
		strHtml = Replace(strHtml, "  ", "&nbsp ") ' Multiple spaces
	Loop
	Do While InStr(strHtml, vbCrLf & vbCrLf) <> 0
		strHtml = Replace(strHtml, vbCrLf & vbCrLf, vbCrLf & "<p>") ' DOS line terminators
	Loop
	Do While InStr(strHtml, vbLf & vbLf) <> 0
		strHtml = Replace(strHtml, vbLf & vbLf, vbCrLf & "<p>") ' Unix line terminators
	Loop
	Do While InStr(strHtml, vbCr & vbCr) <> 0
		strHtml = Replace(strHtml, vbCr & vbCr, vbCrLf & "<p>") ' Apple line terminators
	Loop
	TextHtml = strHtml
End Function

Function NavigationButtons(lngSlideIndex, varBaseNames)
' Supplies the HTML code for back, home, and next navigation buttons
' See http:'www.ericphelps.com/unicode/ for other unicode values
Dim strHtml
	strHtml = ""
	' Back button URL
	strHtml = strHtml & vbTab & "<font size=+2><a href="
	If lngSlideIndex = 0 Then
		strHtml = strHtml & "'" & END_PAGE & "'"
	Else
		strHtml = strHtml & "'" & ArgumentsForThisUrl("?slide=" & (lngSlideIndex - 1)) & "'"
	End If
	If Lcase(Left(BACK_BUTTON, 4)) <> "<img" Then
		'Don't use CSS on real graphic buttons
		strHtml = strHtml & cssStyle()
	End If
	strHtml = strHtml & " title='Previous' tabIndex='2'>" & BACK_BUTTON & "</a></font>&nbsp" & vbCrLf
	'Home button URL
	strHtml = strHtml & vbTab & "<font size=+2><a href="
	strHtml = strHtml & "'" & HOME_PAGE & "'"
	strHtml = strHtml & " title='Home' tabIndex='3'"
	If Lcase(Left(HOME_BUTTON, 4)) <> "<img" Then
		'Don't use CSS on real graphic buttons
		strHtml = strHtml & cssStyle()
	End If
	strHtml = strHtml & ">" & HOME_BUTTON & "</a></font>&nbsp" & vbCrLf
	' Next button URL
	strHtml = strHtml & vbTab & "<font size=+2><a href="
	If lngSlideIndex = Ubound(varBaseNames) Then
		strHtml = strHtml & "'" & END_PAGE & "'"
	Else
		strHtml = strHtml & "'" & ArgumentsForThisUrl("?slide=" & (lngSlideIndex + 1)) & "'"
	End If
	If Lcase(Left(NEXT_BUTTON, 4)) <> "<img" Then
		'Don't use CSS on real graphic buttons
		strHtml = strHtml & cssStyle()
	End If
	' Add id, name, and tabindex attributes to next button so we can give it focus later
	strHtml = strHtml & " title='Next' id='Next' name='Next' tabIndex='1'>" & NEXT_BUTTON & "</a></font>"
	strHtml = strHtml & " <img height=1 width=150>" 'Added to prevent table from collapsing
	strHtml = strHtml &"</td>" & vbCrLf
	NavigationButtons = strHtml
End Function

Function cssStyle()
'Returns style code that can be inserted into an "a" tag to make it into a button
Dim strHtml
	strHtml = ""
	strHtml = strHtml & " style="""
	strHtml = strHtml & "color: black; "
	strHtml = strHtml & "border: 1px solid; "
	strHtml = strHtml & "background-color: gray; "
	strHtml = strHtml & "padding: 2px; "
	strHtml = strHtml & "padding-left: 3px; "
	strHtml = strHtml & "text-decoration: none; "
	strHtml = strHtml & "border-color: silver black black silver; "
	strHtml = strHtml & "margin: 0px; "
	strHtml = strHtml & "text-align: center; "
	strHtml = strHtml & """ "
	cssStyle = strHtml
End Function

Function SoundIE(strSoundUrl, strNextUrl)
	strHtml = ""
	strHtml = strHtml & "	<OBJECT ID='MediaPlayer' width='144' height='45'" & vbCrLf
	strHtml = strHtml & "		classid='CLSID:22D6F312-B0F6-11D0-94AB-0080C74C7E95'" & vbCrLf
	strHtml = strHtml & "		CODEBASE='http:'activex.microsoft.com/activex/controls/mplayer/en/nsmp2inf.cab#Version=6,4,5,715'" & vbCrLf
	strHtml = strHtml & "		standby='Loading Microsoft® Windows® Media Player components...'" & vbCrLf
	strHtml = strHtml & "		type='application/x-oleobject'>" & vbCrLf
	strHtml = strHtml & "		<PARAM NAME='FileName' VALUE='" & strSoundUrl & "'" & vbCrLf
	strHtml = strHtml & "		<PARAM NAME='ShowControls' VALUE='True'>" & vbCrLf
	strHtml = strHtml & "		<EMBED type='application/x-mplayer2'" & vbCrLf
	strHtml = strHtml & "			pluginspage='http:'www.microsoft.com/windows/windowsmedia/download/plugin.aspx'" & vbCrLf
	strHtml = strHtml & "			height='45' width='144'" & vbCrLf
	strHtml = strHtml & "			src='" & strSoundUrl & "'" & vbCrLf
	strHtml = strHtml & "			autostart='True' autoplay='True'" & vbCrLf
	strHtml = strHtml & "			showcontrols='1'" & vbCrLf
	strHtml = strHtml & "			visible='True' hidden='False'>" & vbCrLf
	strHtml = strHtml & "		</EMBED>" & vbCrLf
	strHtml = strHtml & "	</OBJECT></td>" & vbCrLf
	' The following code causes IE to auto-advance if scripting is enabled.
	If strNextUrl <> "" Then ' Don't advance if there is nowhere to advance to!
		strHtml = strHtml & "	<script LANGUAGE='JavaScript' FOR='MediaPlayer' EVENT='PlayStateChange(OldPlayState, NewPlayState)'>" & vbCrLf
		strHtml = strHtml & "		 if(OldPlayState == 2 && NewPlayState == 0) {" & vbCrLf
		strHtml = strHtml & "		 	window.setTimeout('top.location.href=""" & strNextUrl & """', 1000)" & vbCrLf
		strHtml = strHtml & "		 }" & vbCrLf
		strHtml = strHtml & "	</script>" & vbCrLf
	End If
	SoundIE = strHtml
End Function

Function SoundWindows(strSoundUrl)
Dim strHtml
	strHtml = ""
	strHtml = strHtml & "	<EMBED type='application/x-mplayer2'" & vbCrLf
	strHtml = strHtml & "		pluginspage='http:'www.microsoft.com/windows/windowsmedia/download/plugin.aspx'" & vbCrLf
	strHtml = strHtml & "		height='45' width='144'" & vbCrLf
	strHtml = strHtml & "		src='" & strSoundUrl & "'" & vbCrLf
	strHtml = strHtml & "		autostart='True' autoplay='True'" & vbCrLf
	strHtml = strHtml & "		showcontrols='1'" & vbCrLf
	strHtml = strHtml & "		visible='True' hidden='False'>" & vbCrLf
	strHtml = strHtml & "	</EMBED>" & vbCrLf
	SoundWindows = strHtml
End Function

Function SoundGeneric(strSoundUrl)
Dim strHtml, strExtension, dictMimes
	strExtension = Mid(strSoundUrl, InStrRev(strSoundUrl, ".") + 1)
	strExtension = LCase(strExtension)
	Set dictMimes = CreateDictionary("au=audio/basic mid=audio/midi mp3=audio/mpeg wav=audio/wav wma=audio/x-wma", "=", " ")
	strHtml = ""
	strHtml = strHtml & "	<EMBED type='" & dictMimes.Item(strExtension) & "'" & vbCrLf
	strHtml = strHtml & "		height='45' width='144'" & vbCrLf
	strHtml = strHtml & "		src='" & strSoundUrl & "'" & vbCrLf
	strHtml = strHtml & "		autostart='True' autoplay='True'" & vbCrLf
	strHtml = strHtml & "		showcontrols='1'" & vbCrLf
	strHtml = strHtml & "		visible='True' hidden='False'>" & vbCrLf
	strHtml = strHtml & "	</EMBED>" & vbCrLf
	SoundGeneric = strHtml
End Function

Function CreateDictionary(strMultiplyDelimitedString, strItemSeparator, strEntrySeparator)
'Accepts a multiply-delimited string and returns a dictionary. For example
'Set dict=CreateDictionary("counts=9.3&name=Eric Phelps", "=", "&") becomes available like this...
'dict.Item("counts") will return "9.3", and dict.Item("name") returns "Eric Phelps".
'Likewise dict.Exists("counts") is True and dict.Exists("xyftc") is False.
Dim strQuery 'As String
Dim strName 'As String
Dim strValue 'As String
Dim dict 'As Object
	Set dict = CreateObject("Scripting.Dictionary")
	strQuery = strMultiplyDelimitedString
	Do While strQuery <> ""
		strName = Left(strQuery, InStr(strQuery, strItemSeparator) - 1)
		strQuery = Mid(strQuery, InStr(strQuery, strItemSeparator) + Len(strItemSeparator))
		If InStr(strQuery, strEntrySeparator) = 0 Then
			strValue = strQuery
			strQuery = ""
		Else
			strValue = Left(strQuery, InStr(strQuery, strEntrySeparator) - 1)
			strQuery = Mid(strQuery, InStr(strQuery, strEntrySeparator) + Len(strEntrySeparator))
		End If
		'Allow for multiple items by tab-delimiting them
		If dict.Exists(strName) Then
			dict.Item(strName) = dict.Item(strName) & vbTab & strValue
		Else
			dict.Add strName, strValue
		End If
	Loop
	Set CreateDictionary = dict
	Set dict = Nothing
End Function

%></a></p>
<p>&nbsp;</p>