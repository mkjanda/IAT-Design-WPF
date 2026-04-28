<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>
  <xsl:template match="/ResultDocument">
    <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
      <Default Extension="bin" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.printerSettings"/>
      <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
      <Default Extension="xml" ContentType="application/xml"/>
      <Default Extension="jpg" ContentType="image/jpeg"/>
      <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
      <Override PartName="/xl/worksheets/Summary.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
      <xsl:for-each select="distinct-values(//IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending" />
        <xsl:element name="Override">
          <xsl:attribute name="PartName" select="concat('/xs/worksheets/Item', ., '.xml')" />
          <xsl:attribute name="ContentType" select="'application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml'" />
        </xsl:element>
      </xsl:for-each>
      <Override PartName="/xl/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
      <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
      <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
      <xsl:for-each select="distinct-values(//IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending" />
        <xsl:element name="Override">
          <xsl:attribute name="PartName" select="concat('/xs/drawings/ItemNum', ., '.xml')" />
          <xsl:attribute name="ContentType" select="'application/vnd.openxmlformats-officedocument.drawing+xml'" />
        </xsl:element>
      </xsl:for-each>
      <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
      <Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
    </Types>
  </xsl:template>
</xsl:stylesheet>