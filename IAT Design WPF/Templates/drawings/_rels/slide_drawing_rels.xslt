<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/package/2006/relationships"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:variable name="rd" select="/" />

  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:template match="/ResultDocument">
    <xsl:variable name="itrCount" select="$rd//IterationInfo/IterationCount" />
    <xsl:variable name="itrVal" select="$rd//IterationInfo/IterationValue" />
    <xsl:element name="Relationships">
      <xsl:element name="Relationship">
        <xsl:attribute name="Id" select="'rId1'" />
        <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/image'" />
        <xsl:attribute name="Target" select="concat('../media/image', xs:integer($itrVal) + count($rd//TitlePage/PageHeights), '.jpg')" />
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>