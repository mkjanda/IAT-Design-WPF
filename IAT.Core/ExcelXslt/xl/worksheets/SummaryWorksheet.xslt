<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                xmlns:x14ac="http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"
                xmlns:mine="http://www.iatsoftware.net/dummy"
                version="2.0"
                exclude-result-prefixes="xs mine">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:variable name="rd" select="." />

  <xsl:variable name="alphabet">
    <xsl:value-of select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
  </xsl:variable>

  <xsl:function name="mine:numToCol">
    <xsl:param name="num" />
    <xsl:if test="xs:integer($num) le string-length($alphabet)">
      <xsl:value-of select="substring($alphabet, xs:integer($num), 1)" />
    </xsl:if>
    <xsl:if test="(xs:integer($num) gt string-length($alphabet)) and (xs:integer($num) le (string-length($alphabet) * string-length($alphabet)) + string-length($alphabet))">
      <xsl:variable name="loNdx" select="if ((xs:integer($num) mod string-length($alphabet)) eq 0) then (string-length($alphabet)) else (xs:integer($num) mod string-length($alphabet)) " />
      <xsl:variable name="hiNdx" select="if ((xs:integer($num) - $loNdx) div string-length($alphabet) eq 0) then (1) else ((xs:integer($num) - $loNdx) div string-length($alphabet))" />
      <xsl:value-of select="concat(substring($alphabet, $hiNdx, 1), substring($alphabet, $loNdx, 1))" />
    </xsl:if>
    <xsl:if test="xs:integer($num) gt (string-length($alphabet) * string-length($alphabet)) + string-length($alphabet)">
      <xsl:variable name="loNdx" select="if ((xs:integer($num) mod string-length($alphabet)) eq 0) then (string-length($alphabet)) else (xs:integer($num) mod string-length($alphabet)) " />
      <xsl:variable name="modMid" select="if ((((xs:integer($num) - $loNdx) mod ((string-length($alphabet)) * (string-length($alphabet))))) eq 0) then (-1) else ( if ((xs:integer($num) mod (string-length($alphabet) * string-length($alphabet))) eq 0) then (-2) else (xs:integer($num) mod (string-length($alphabet) * string-length($alphabet))))" />
      <xsl:variable name="midNdx" select="if ($modMid eq -1) then (string-length($alphabet)) else (if ($modMid eq -2) then (string-length($alphabet) - 1) else (if (($modMid - $loNdx) div string-length($alphabet) eq 0) then (1) else ((xs:integer($modMid) - $loNdx) div string-length($alphabet))))" />
      <xsl:variable name="modHi" select="if (xs:integer($num) mod (string-length($alphabet) * string-length($alphabet)) eq 0) then (xs:integer($num) - 1) else ( if ($midNdx eq string-length($alphabet)) then (xs:integer($num) - (string-length($alphabet) * string-length($alphabet))) else (xs:integer($num)))" />
      <xsl:variable name="hiNdx" select="floor(xs:integer($modHi) div (string-length($alphabet) * string-length($alphabet)))" />
      <xsl:value-of select="concat(substring($alphabet, $hiNdx, 1), substring($alphabet, $midNdx, 1), substring($alphabet, $loNdx, 1))" />
    </xsl:if>
  </xsl:function>

  <xsl:variable name="hasTokens" select="count(//TestResult/Token) gt 0" />

  <xsl:template match="/ResultDocument">
    <xsl:element name="worksheet">
      <xsl:namespace name="r" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'" />
      <xsl:namespace name="mc" select="'http://schemas.openxmlformats.org/markup-compatibility/2006'" />
      <xsl:namespace name="x14ac" select="'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'" />
      <xsl:attribute name="mc:Ignorable" select="'x14ac'" />
      <xsl:variable name="dataDim">
        <mine:width>
          <xsl:value-of select="1 + count(SurveyDesign/SurveyFormat/Questions[@ResponseType ne 'None']) + (if ($hasTokens) then 1 else 0)" />
        </mine:width>
        <mine:height>
          <xsl:value-of select="count(TestResult)" />
        </mine:height>
      </xsl:variable>
      <xsl:element name="dimension">
        <xsl:variable name="gridspan">
          <xsl:element name="mine:MinRow">1</xsl:element>
          <xsl:element name="mine:MaxRow">
            <xsl:value-of select="$dataDim/mine:height" />
          </xsl:element>
          <xsl:element name="mine:MinCol">
            <xsl:value-of select="'A'" />
          </xsl:element>
          <xsl:element name="mine:MaxCol">
            <xsl:value-of select="mine:numToCol($dataDim/mine:width)" />
          </xsl:element>
        </xsl:variable>
        <xsl:attribute name="ref" select="concat($gridspan/mine:MinCol, $gridspan/mine:MinRow, ':', $gridspan/mine:MaxCol, $gridspan/mine:MaxRow)" />
      </xsl:element>
      <sheetViews>
        <sheetView workbookViewId="0" />
      </sheetViews>
      <sheetFormatPr defaultRowHeight="15" x14ac:dyDescent="0.25"/>
      <cols>
        <xsl:element name="col">
          <xsl:attribute name="min" select="$dataDim/mine:width" />
          <xsl:attribute name="max" select="$dataDim/mine:width" />
          <xsl:attribute name="width" select="15" />
          <xsl:attribute name="customWidth" select="1" />
        </xsl:element>
      </cols>
      <xsl:element name="sheetData">
        <xsl:for-each select="$rd//TestResult">
          <xsl:variable name="resultNum" select="position()" />
          <xsl:variable name="testResultNode" select="." />
          <xsl:element name="row">
            <xsl:attribute name="r" select="position()" />
            <xsl:attribute name="spans" select="concat('1:', $dataDim/mine:width)" />
            <xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
            <xsl:if test="exists(Token)">
              <xsl:element name="c">
                <xsl:attribute name="r" select="concat('A', $resultNum)" />
                <xsl:attribute name="s" select="'1'" />
                <xsl:attribute name="t" select="'s'" />
                <xsl:element name="v">
                  <xsl:value-of select="mine:getSSNdx(Token)" />
                </xsl:element>
              </xsl:element>
            </xsl:if>
            <xsl:apply-templates select="SurveyResults[xs:integer(@ElementNum) lt xs:integer($testResultNode/IATResult/@ElementNum)]">
              <xsl:with-param name="rowNum" select="$resultNum" />
              <xsl:with-param name="testResult" select="." />
            </xsl:apply-templates>
            <xsl:apply-templates select="IATResult">
              <xsl:with-param name="rowNum" select="$resultNum" />
              <xsl:with-param name="testResult" select="." />
            </xsl:apply-templates>
            <xsl:apply-templates select="SurveyResults[xs:integer(@ElementNum) gt xs:integer($testResultNode/IATResult/@ElementNum)]">
              <xsl:with-param name="rowNum" select="$resultNum" />
              <xsl:with-param name="testResult" select="." />
            </xsl:apply-templates>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="SurveyResults">
    <xsl:param name="rowNum" />
    <xsl:param name="testResult" />
    <xsl:variable name="surveyResults" select="." />
    <xsl:for-each select="Answer">
      <xsl:variable name="answer" select="." />
      <xsl:variable name="colNum" select="count($testResult/child::SurveyResults[xs:integer(@ElementNum) lt xs:integer($answer/parent::SurveyResults/@ElementNum)]/Answer) + count($answer/preceding-sibling::Answer) + 1 + (if (xs:integer(parent::SurveyResults/@ElementNum) gt (count(distinct-values(parent::SurveyResults/preceding-sibling::SurveyResults)) + 1)) then 1 else 0) + (if ($hasTokens) then 1 else 0)" />
      <xsl:variable name="testElemNum" select="xs:integer($surveyResults/@ElementNum)" />
      <xsl:element name="c">
        <xsl:variable name="col" select="mine:numToCol($colNum)" />
        <xsl:attribute name="r" select="concat($col, $rowNum)" />
        <xsl:variable name="respType" select="$rd//SurveyFormat[xs:integer(@ElementNum) eq $testElemNum]/Questions[@ResponseType ne 'None'][count($answer/preceding-sibling::Answer) + 1]/@ResponseType" />
        <xsl:variable name="numRespTypes" select="tokenize('Likert WeightedMultiple BoundedNum Multiple', ' ')" />
        <xsl:variable name="sharedStrRespTypes" select="tokenize('MultiBoolean Date RegEx BoundedLength Boolean FixedDig', ' ')" />
        <xsl:choose>
          <xsl:when test="count(distinct-values($respType[.=$numRespTypes])) gt 0">
            <xsl:attribute name="s" select="'2'" />
            <xsl:element name="v">
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:when>
          <xsl:when test="count(distinct-values($respType[.=$sharedStrRespTypes])) gt 0">
            <xsl:attribute name="s" select="'1'" />
            <xsl:attribute name="t" select="'s'" />
            <xsl:element name="v">
              <xsl:value-of select="mine:getSSNdx(xs:string(.))" />
            </xsl:element>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="s" select="'2'" />
            <xsl:element name="v">
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="IATResult">
    <xsl:param name="rowNum" />
    <xsl:param name="testResult" />
    <xsl:variable name="ir" select="$testResult/IATResult" />
    <xsl:variable name="colNum" select="count($testResult/IATResult/preceding-sibling::SurveyResults[xs:integer(@ElementNum) lt xs:integer($ir/@ElementNum)]/Answer) + (if ($hasTokens) then 1 else 0) + 1" />
    <xsl:variable name="col" select="mine:numToCol($colNum)" />
    <xsl:element name="c">
      <xsl:attribute name="r" select="concat($col, $rowNum)" />
      <xsl:if test="IATScore eq 'NaN'">
        <xsl:attribute name="s" select="'1'" />
        <xsl:attribute name="t" select="'s'" />
        <xsl:element name="v">
          <xsl:value-of select="mine:getSSNdx('Unscored')" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="IATScore ne 'NaN'">
        <xsl:element name="v">
          <xsl:attribute name="s" select="'2'" />
          <xsl:value-of select="IATScore" />
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:function name="mine:getSSNdx">
    <xsl:param name="str" />
    <xsl:value-of select="xs:integer($rd//Addendum/SharedStrings/SharedString[.=$str]/@Index) - 1" />
  </xsl:function>

</xsl:stylesheet>