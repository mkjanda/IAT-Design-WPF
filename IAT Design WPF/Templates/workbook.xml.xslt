<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>
  <xsl:template match="/ResultDocument">
    <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
              xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
      <fileVersion appName="xl" lastEdited="5" lowestEdited="5" rupBuild="9303"/>
      <workbookPr defaultThemeVersion="124226"/>
      <bookViews>
        <workbookView xWindow="120" yWindow="90" windowWidth="21075" windowHeight="8250"/>
      </bookViews>
      <sheets>
        <sheet name="Title" sheetId="1" r:id="rId1"/>
        <sheet name="Summary" sheetId="2" r:id="rId2"/>
        <sheet name="Latencies" sheetId="3" r:id="rId3"/>
        <xsl:for-each select="distinct-values(TestResult/IATResult/IATResponse/ItemNum)">
          <xsl:sort select="xs:integer(.)" order="ascending" />
          <xsl:element name="sheet">
            <xsl:attribute name="name" select="concat('Item ', .)"/>
            <xsl:attribute name="sheetId" select="xs:integer(position()) + 3"/>
            <xsl:attribute name="r:id" select="concat('rId', position() + 3)"/>
          </xsl:element>
        </xsl:for-each>
      </sheets>
      <calcPr calcId="145621"/>
    </workbook>
  </xsl:template>
</xsl:stylesheet>