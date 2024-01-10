# RocketEventsAPI

Event system is a sub-system of the RocketDirectory system.

The setup of the defaults is done is the **SystemDefaults.rules** file.

## SQL Index
Sort Index is required for "publisheddate":  
**SystemDefaults.rules section:**
```
	<sqlindex list="true">
		<genxml>
			<systemkey>rocketeventsapi</systemkey>
			<ref>publisheddate</ref>
			<xpath>genxml/textbox/publisheddate</xpath>
			<typecode>rocketeventsapiART</typecode>
		</genxml>
	</sqlindex>
```
## Sort Order is by publisheddate
```
sqlorderby-publisheddate | order by publisheddate.GUIDKey desc
```
**SystemDefaults.rules section:**
```
<sqlorderby>
	<product>
		<publisheddate>
			order by publisheddate.GUIDKey desc
		</publisheddate>
	</product>
</sqlorderby>
```

## Menu Provider
The Menu Provider adds the category menu to the DDRMenu.  
**SystemDefaults.rules section:**
```
<menuprovider>
	<assembly>RocketDirectoryAPI</assembly>
	<namespaceclass>RocketDirectoryAPI.Components.MenuDirectory</namespaceclass>
</menuprovider>
```
## Article Search Filter
The article search filter is used b ythe RocketDirectory search system.  It can be edited in the Admin Settings section of the system.   
```
and
(
	(
		isnull(articlename.GUIDKey,'') like '%{searchtext}%'
		or isnull([XMLData].value('(genxml/textbox/articleref)[1]','nvarchar(max)'),'') like '%{searchtext}%'
		or isnull([XMLData].value('(genxml/lang/genxml/textbox/articlekeywords)[1]','nvarchar(max)'),'') like '%{searchtext}%'
	)
	and
	(
		([XMLData].value('(genxml/textbox/publisheddate)[1]','date') >= convert(date,'{searchdate1}') or '{searchdate1}' = '')
		and
		([XMLData].value('(genxml/textbox/publisheddate)[1]','date') <= convert(date,'{searchdate2}') or '{searchdate2}' = '')
	)
)
```
**SystemDefaults.rules section:**
```
  <sqlfilter>
    <article>
		<![CDATA[
		and
		(
		(
		isnull(articlename.GUIDKey,'') like '%{searchtext}%'
		or isnull([XMLData].value('(genxml/textbox/articleref)[1]','nvarchar(max)'),'') like '%{searchtext}%'
		or isnull([XMLData].value('(genxml/lang/genxml/textbox/articlekeywords)[1]','nvarchar(max)'),'') like '%{searchtext}%'
		)
		and
		(
		([XMLData].value('(genxml/textbox/publisheddate)[1]','date') >= convert(date,'{searchdate1}') or '{searchdate1}' = '')
		and
		([XMLData].value('(genxml/textbox/publisheddate)[1]','date') <= convert(date,'{searchdate2}') or '{searchdate2}' = '')
		)
		)
		]]>
	</article>    
  </sqlfilter>
```