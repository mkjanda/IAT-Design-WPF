<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"
                xmlns:mine="http://www.iatsoftware.net/dummy"
                version="2.0"
                exclude-result-prefixes="xs mine">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes" />
  <xsl:template match="/ResultDocument">
    <Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
      <Application>Microsoft Excel</Application>
      <DocSecurity>0</DocSecurity>
      <ScaleCrop>false</ScaleCrop>
      <HeadingPairs>
        <vt:vector size="2" baseType="variant">
          <vt:variant>
            <vt:lpstr>Worksheets</vt:lpstr>
          </vt:variant>
          <vt:variant>
            <vt:i4>
              <xsl:value-of select="count(distinct-values(//IATResponse/ItemNum)) + 3"/>
            </vt:i4>
          </vt:variant>
        </vt:vector>
      </HeadingPairs>
      <TitlesOfParts>
        <xsl:variable name="sheetNames">
          <xsl:element name="mine:Sheet">
            <xsl:value-of select="'Title'" />
          </xsl:element>
          <xsl:element name="mine:Sheet">
            <xsl:value-of select="'Summary'" />
          </xsl:element>
          <xsl:element name="mine:Sheet">
            <xsl:value-of select="'Latencies'" />
          </xsl:element>
          <xsl:for-each select="distinct-values(//IATResponse/ItemNum)">
            <xsl:sort select="xs:integer(.)" order="ascending" />
            <xsl:element name="mine:Sheet">
              <xsl:value-of select="concat('Item ', .)" />
            </xsl:element>
          </xsl:for-each>
        </xsl:variable>
        <xsl:element name="vt:vector">
          <xsl:attribute name="size" select="count($sheetNames/mine:Sheet)" />
          <xsl:attribute name="baseType" select="'lpstr'" />
          <xsl:for-each select="$sheetNames/mine:Sheet">
            <xsl:element name="vt:lpstr">
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </TitlesOfParts>
      <Company>Microsoft</Company>
      <LinksUpToDate>false</LinksUpToDate>
      <SharedDoc>false</SharedDoc>
      <HyperlinksChanged>false</HyperlinksChanged>
      <AppVersion>14.0300</AppVersion>
    </Properties>
  </xsl:template>
</xsl:stylesheet>