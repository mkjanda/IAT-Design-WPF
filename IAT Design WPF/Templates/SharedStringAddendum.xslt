<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:mine="http://www.iatsoftware.net/dummy"
                version="2.0"
                exclude-result-prefixes="xs mine">

  <xsl:variable name="root" select="/ResultDocument" />

  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:template match="/ResultDocument">

    <xsl:variable name="stringList" select="mine:getSharedStrings()" />
    <xsl:element name="SharedStrings">
      <xsl:for-each select="$stringList/mine:string">
        <xsl:element name="SharedString">
          <xsl:attribute name="Index" select="position()" />
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>


  <xsl:function name="mine:getSharedStrings">
    <xsl:variable name="staticStrings">
      <mine:heading>Result Set</mine:heading>
      <mine:heading>IAT Score</mine:heading>
      <mine:heading>Error Count</mine:heading>
      <mine:heading>Latencies</mine:heading>
      <mine:heading>Block 1</mine:heading>
      <mine:heading>Block 2</mine:heading>
      <mine:heading>Block 3</mine:heading>
      <mine:heading>Block 4</mine:heading>
      <mine:heading>Block 5</mine:heading>
      <mine:heading>Block 6</mine:heading>
      <mine:heading>Block 7</mine:heading>
      <mine:heading>Item #</mine:heading>
      <mine:heading>Latency (ms)</mine:heading>
    </xsl:variable>
    <xsl:variable name="Strings">
      <xsl:for-each select="$staticStrings/mine:heading">
        <xsl:element name="mine:string">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:for-each>
      <xsl:variable name="strRespTypes">

        <xsl:for-each select="tokenize('MultiBoolean Date RegEx BoundedLength FixedDig Boolean', ' ')">
          <xsl:element name="textType">
            <xsl:value-of select="." />
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>
      <xsl:if test="some $answer in $root//TestResult/SurveyResults/Answer satisfies $answer eq 'Unaswered'">
        <xsl:element name="mine:string">Unanswered</xsl:element>
      </xsl:if>
      <xsl:if test="some $answer in $root//TestResult/SurveyResults/Answer satisfies $answer eq 'NULL'">
        <xsl:element name="mine:string">NULL</xsl:element>
      </xsl:if>
      <xsl:for-each select="for $i in 1 to count($root//SurveyDesign//Questions[@ResponseType ne 'None']) return $i">
        <xsl:variable name="questNum" select="xs:integer(.)" />
        <xsl:if test="$root//SurveyDesign//Questions[@ResponseType ne 'None'][$questNum]/@ResponseType[.=$strRespTypes/child::*]">
          <xsl:for-each select="$root//TestResult/SurveyResults/Answer[(position() eq xs:integer($questNum)) and (string-length(.) gt 0)]">
            <xsl:if test="not (.=tokenize('Unanswered,NULL', ','))">
              <xsl:element name="mine:string">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:if>
          </xsl:for-each>
        </xsl:if>
      </xsl:for-each>
      <xsl:if test="some $score in $root//IATResult/IATScore satisfies $score eq 'NaN'">
        <xsl:element name="mine:string">
          <xsl:value-of select="'Unscored'" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="exists($root//TokenName)">
        <xsl:element name="mine:string">
          <xsl:value-of select="$root//TokenName" />
        </xsl:element>
      </xsl:if>
      <xsl:for-each select="distinct-values($root//TestResult/Token)">
        <xsl:element name="mine:string">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="distinctStrings">
      <xsl:for-each select="distinct-values($Strings/mine:string)">
        <xsl:element name="mine:string">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:sequence select="$distinctStrings" />
  </xsl:function>
</xsl:stylesheet>