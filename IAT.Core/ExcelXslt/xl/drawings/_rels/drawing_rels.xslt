<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/package/2006/relationships"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:variable name="rd" select="/" />

  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>
  <xsl:template match="/ResultDocument">
    <xsl:element name="XMLWrapper" namespace="">
      <xsl:element name="WrappedElement" xmlns="">
        <xsl:attribute name="Path" select="'/xl/drawings/_rels/drawing1.xml.rels'" />
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <xsl:for-each select="$rd//TitlePage/PageHeights">
            <xsl:element name="Relationship">
              <xsl:attribute name="Id" select="concat('rId', position())" />
              <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/image'" />
              <xsl:attribute name="Target" select="concat('../media/image', position(), '.png')" />
            </xsl:element>
          </xsl:for-each>
        </Relationships>
      </xsl:element>
      <xsl:for-each select="distinct-values($rd//IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending" />
        <xsl:variable name="itemNum" select="." />
        <xsl:element name="WrappedElement" namespace="">
          <xsl:attribute name="Path" select="concat('/xl/drawings/_rels/drawing', position() + 1, '.xml.rels')" />
          <xsl:element name="Relationships">
            <xsl:element name="Relationship">
              <xsl:attribute name="Id" select="'rId1'" />
              <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/image'" />
              <xsl:attribute name="Target" select="concat('../media/image', xs:integer($rd//ItemSlide[ItemNum eq $itemNum]/SlideNum) + count($rd//TitlePage/PageHeights), '.jpg')" />
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>