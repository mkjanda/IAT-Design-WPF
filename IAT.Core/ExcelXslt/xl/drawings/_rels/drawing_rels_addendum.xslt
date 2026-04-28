<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/package/2006/relationships"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:variable name="rd" select="/" />

  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:template match="/ResultDocument">
    <xsl:element name="NumTitlePageImages">
      <xsl:value-of select="count(//TitlePage/PageHeights)" />
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>