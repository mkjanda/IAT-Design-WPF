<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/package/2006/relationships"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:template match="ResultDocument">
    <xsl:variable name="itrCount" select="xs:integer(//Addendum/IterationInfo/IterationCount)" />
    <Relationships>
      <xsl:element name="Relationship">
        <xsl:attribute name="Id" select="'rId1'" />
        <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing'" />
        <xsl:variable name="missingPrecedingItems">
          <xsl:variable name="precedingItems" select="distinct-values(//IATResponse/ItemNum[xs:integer(.) lt $itrCount])" />
          <xsl:value-of select="$itrCount - count($precedingItems)" />
        </xsl:variable>
        <xsl:attribute name="Target" select="concat('../drawings/drawing', xs:string($itrCount + 2 - xs:integer($missingPrecedingItems)), '.xml')" />
      </xsl:element>
    </Relationships>
  </xsl:template>
</xsl:stylesheet>