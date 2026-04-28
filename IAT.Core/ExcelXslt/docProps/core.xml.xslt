<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dc="http://purl.org/dc/elements/1.1/"
                xmlns:dcterms="http://purl.org/dc/terms/"
                xmlns:dcmitype="http://purl.org/dc/dcmitype/"
                xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>
  <xsl:template match="/ResultDocument">
    <cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
                       xmlns:dc="http://purl.org/dc/elements/1.1/"
                       xmlns:dcterms="http://purl.org/dc/terms/"
                       xmlns:dcmitype="http://purl.org/dc/dcmitype/"
                       xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <xsl:element name="dc:creator">
        <xsl:value-of select="TestAuthor"/>
      </xsl:element>
      <xsl:element name="cp:lastModifiedBy">
        <xsl:value-of select="TestAuthor"/>
      </xsl:element>
      <xsl:element name="dcterms:created">
        <xsl:attribute name="xsi:type" select="'dcterms:W3CDTF'"/>
        <xsl:value-of select="RetrievalTime"/>
      </xsl:element>
      <xsl:element name="dcterms:modified">
        <xsl:attribute name="xsi:type" select="'dcterms:W3CDTF'"/>
        <xsl:value-of select="RetrievalTime"/>
      </xsl:element>
    </cp:coreProperties>
  </xsl:template>
</xsl:stylesheet>