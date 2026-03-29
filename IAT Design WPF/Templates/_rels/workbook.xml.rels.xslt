<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes" />
  <xsl:template match="/ResultDocument">
    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml" />
      <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
      <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet3.xml" /> 
      <xsl:for-each select="distinct-values(TestResult/IATResult/IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending" />
        <xsl:element name="Relationship">
          <xsl:attribute name="Id" select="concat('rId', position() + 3)"/>
          <xsl:attribute name="Type"
                         select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet'"/>
          <xsl:attribute name="Target" select="concat('worksheets/sheet', position() + 3, '.xml')"/>
        </xsl:element>
      </xsl:for-each>
      <xsl:element name="Relationship">
        <xsl:attribute name="Id" select="concat('rId', xs:string(count(distinct-values(TestResult/IATResult/IATResponse/ItemNum)) + 6))" />
        <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings'" />
        <xsl:attribute name="Target" select="'sharedStrings.xml'" />
      </xsl:element>
      <xsl:element name="Relationship">
        <xsl:attribute name="Id" select="concat('rId', xs:string(count(distinct-values(TestResult/IATResult/IATResponse/ItemNum)) + 5))" />
        <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles'" />
        <xsl:attribute name="Target" select="'styles.xml'" />
      </xsl:element>
      <xsl:element name="Relationship">
        <xsl:attribute name="Id" select="concat('rId', xs:string(count(distinct-values(TestResult/IATResult/IATResponse/ItemNum)) + 4))" />
        <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme'" />
        <xsl:attribute name="Target" select="'theme/theme1.xml'" />
      </xsl:element>
    </Relationships>
  </xsl:template>
</xsl:stylesheet>