<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/package/2006/relationships"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:template match="ResultDocument">
    <xsl:element name="XmlWrapper" namespace="">
      <xsl:element name="WrappedElement" namespace="">
        <xsl:attribute name="Path" select="'/xl/worksheets/_rels/sheet1.xml.rels'" />
        <Relationships>
          <xsl:element name="Relationship">
            <xsl:attribute name="Id" select="'rId1'" />
            <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing'" />
            <xsl:attribute name="Target" select="'../drawings/drawing1.xml'" />
          </xsl:element>
        </Relationships>
      </xsl:element>
      <xsl:for-each select="distinct-values(//TestResult/IATResult/IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending" />
        <xsl:element name="WrappedElement" namespace="">
          <xsl:attribute name="Path" select="concat('/xl/worksheets/_rels/sheet', position() + 3, '.xml.rels')" />
          <Relationships>
            <xsl:element name="Relationship">
              <xsl:attribute name="Id" select="'rId1'" />
              <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing'" />
              <xsl:attribute name="Target" select="concat('../drawings/drawing', position() + 1, '.xml')" />
            </xsl:element>
          </Relationships>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>