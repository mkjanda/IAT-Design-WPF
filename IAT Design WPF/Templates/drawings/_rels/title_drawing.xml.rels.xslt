<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"
                xmlns:a="http://schemass.openxmlformats.org/drawingml/2006/main"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                xmlns:x14ac="http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"
                xmlns:mine="http://www.iatsoftware.net/dummy"
                version="2.0"
                exclude-result-prefixes="xs mine">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>
  <xsl:variable name="alphabet">
    <xsl:value-of select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
  </xsl:variable>
  <xsl:variable name="root">
    <xsl:copy-of select="/" />
  </xsl:variable>
  <xsl:template match="/ResultDocument">
    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
      <xsl:for-each select="TitlePage/PageHeights">
        <xsl:element name="Relationship">
          <xsl:attribute name="Id" select="concat('rId', position())" />
          <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/image'" />
          <xsl:attribute name="Target" select="concat('../media/titlepage', position(), '.png')" />
        </xsl:element>
      </xsl:for-each>
    </Relationships>
  </xsl:template>
</xsl:stylesheet>
