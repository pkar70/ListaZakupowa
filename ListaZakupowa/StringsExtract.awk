# na wejsciu: plik XAML
# na wyjsciu: co powinno byc dodane do resources.resw

#   <data name="resAboutRate.Label" xml:space="preserve">
#    <value>Rate</value>
#  </data>

{
 if($0 !~ "x:Uid")  next

 sUid = extractData($0,"x:Uid")
 if(sUid != "")
	{
	tryProperty($0, sUid, "OffContent")
	tryProperty($0, sUid, "OnContent")
	tryProperty($0, sUid, "Header")
	tryProperty($0, sUid, "Label")
	tryProperty($0, sUid, "PlaceholderText")
	tryProperty($0, sUid, " Text")
	tryProperty($0, sUid, " Content")
	}
}

function extractData(sLine, sItem)
{
# print "### extractData(" sLine "," sItem ")\n"
# print "### extractData(," sItem ")\n"
 sItem = sItem "=\""
 iInd = index(sLine,sItem)
# print "### iInd=" iInd "\n"
 if(iInd<1) return ""
 sTmp = substr(sLine,iInd+length(sItem))
# print "### sTMp=" sTmp "\n"
 iInd = index(sTmp,"\"")
# print "### iInd=" iInd "\n"
 if(iInd<0) return ""
# print "### returning: " substr(sTmp,0,iInd-1) "\n"
 return substr(sTmp,0,iInd-1) 
}

function tryProperty(sLine, sUid, sProperty)
{
 sTmp = extractData(sLine, sProperty)
 if(sTmp == "") return
 if(length(sTmp) < 2) return
 gsub(" ", "",sProperty)
 printf("<data name=\"%s.%s\" xml:space=\"preserve\">\r\n", sUid, sProperty)
 printf("<value>%s</value>\r\n", sTmp)
 printf("</data>\r\n")
 
}