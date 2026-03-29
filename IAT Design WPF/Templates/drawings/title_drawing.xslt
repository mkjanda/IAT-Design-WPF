<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				        xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
				        version="2.0"
                exclude-result-prefixes="r xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:variable name="rd" select="/" />



  <xsl:template match="//ResultDocument">
    <xdr:wsDr xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
      <xsl:for-each select="$rd//TitlePage/PageHeights">
        <xdr:twoCellAnchor editAs="oneCell">
          <xdr:from>
            <xdr:col>1</xdr:col>
            <xdr:colOff>0</xdr:colOff>
            <xsl:element name="xdr:row">
              <xsl:value-of select="ceiling((sum(preceding-sibling::PageHeights) div 20))" />
            </xsl:element>
            <xdr:rowOff>0</xdr:rowOff>
          </xdr:from>
          <xdr:to>
            <xdr:col>16</xdr:col>
            <xdr:colOff>0</xdr:colOff>
            <xdr:row>
              <xsl:value-of select="ceiling((xs:integer(.) + sum(preceding-sibling::PageHeights)) div 20)"/>
            </xdr:row>
            <xdr:rowOff>0</xdr:rowOff>
          </xdr:to>
          <xdr:pic>
            <xdr:nvPicPr>
              <xsl:element name="xdr:cNvPr">
                <xsl:attribute name="id" select="position() + 1" />
                <xsl:attribute name="name" select="concat('Picture ', position())" />
              </xsl:element>
              <xdr:cNvPicPr>
                <a:picLocks noChangeAspect="1"/>
              </xdr:cNvPicPr>
            </xdr:nvPicPr>
            <xdr:blipFill>
              <xsl:element name="a:blip">
                <xsl:namespace name="r" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'" />
                <xsl:attribute name="r:embed" select="concat('rId', position())" />
                <a:extLst>
                  <a:ext uri="{{28A0092B-C50C-407E-A947-70E740481C1C}}">
                    <a14:useLocalDpi xmlns:a14="http://schemas.microsoft.com/office/drawing/2010/main" val="0"/>
                  </a:ext>
                </a:extLst>
              </xsl:element>
              <a:stretch>
                <a:fillRect/>
              </a:stretch>
            </xdr:blipFill>
            <xdr:spPr>
              <a:xfrm>
                <a:off x="0" y="0"/>
                <xsl:element name="a:ext">
                  <xsl:attribute name="cx" select="'9600000'" />
                  <xsl:attribute name="cy" select="xs:integer(.) * 9600" />
                </xsl:element>
              </a:xfrm>
              <a:prstGeom prst="rect">
                <a:avLst/>
              </a:prstGeom>
            </xdr:spPr>
          </xdr:pic>
          <xdr:clientData/>
        </xdr:twoCellAnchor>
      </xsl:for-each>
    </xdr:wsDr>

  </xsl:template>
</xsl:stylesheet>
