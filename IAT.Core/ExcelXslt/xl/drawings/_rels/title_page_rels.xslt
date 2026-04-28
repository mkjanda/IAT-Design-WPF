<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/package/2006/relationships"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:variable name="rd" select="/" />

  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:template match="/ResultDocument">
    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
      <xsl:for-each select="$rd//TitlePage/PageHeights">
        <xsl:element name="Relationship">
          <xsl:attribute name="Id" select="concat('rId', position())" />
          <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/image'" />
          <xsl:attribute name="Target" select="concat('../media/image', position(), '.png')" />
        </xsl:element>
      </xsl:for-each>
    </Relationships>
  </xsl:template>
</xsl:stylesheet>
