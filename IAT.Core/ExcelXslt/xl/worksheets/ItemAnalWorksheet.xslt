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

  <xsl:variable name="tableOffset">
    <mine:x>11</mine:x>
    <mine:y>5</mine:y>
  </xsl:variable>

  <xsl:variable name="hasToken" select="exists(//TokenName)" />

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

  <xsl:template match="/ResultDocument">
    <xsl:variable name="ItemNum" select="$rd//IterationInfo/IterationCount" />
    <xsl:variable name="SheetNum" select="$rd//IterationInfo/IterationValue" />
      <xsl:element name="worksheet">
        <xsl:namespace name="r"
                       select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'"/>
        <xsl:namespace name="mc"
                       select="'http://schemas.openxmlformats.org/markup-compatibility/2006'"/>
        <xsl:namespace name="x14ac"
                       select="'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'"/>
        <xsl:attribute name="mc:Ignorable" select="'x14ac'"/>
        <xsl:variable name="dataDim">
          <mine:width>
            <xsl:value-of select="max((8, for $n in $rd//TestResult[some $in in IATResult/IATResponse/ItemNum satisfies xs:integer($in) eq xs:integer($ItemNum)] return count($n/IATResult/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)]) + 2)) + (if ($hasToken) then 1 else 0)"/>
          </mine:width>
          <mine:height>
            <xsl:value-of select="count($rd//TestResult) + 1"/>
          </mine:height>
        </xsl:variable>
        <xsl:variable name="gridspan">
          <xsl:element name="mine:MinRow">
            <xsl:value-of select="$tableOffset/mine:y"/>
          </xsl:element>
          <xsl:element name="mine:MaxRow">
            <xsl:value-of select="$tableOffset/mine:y + $dataDim/mine:height - 1"/>
          </xsl:element>
          <xsl:element name="mine:MinCol">
            <xsl:value-of select="mine:numToCol($tableOffset/mine:x)"/>
          </xsl:element>
          <xsl:element name="mine:MaxCol">
            <xsl:value-of select="mine:numToCol(xs:integer($tableOffset/mine:x) + xs:integer($dataDim/mine:width) - 1)"/>
          </xsl:element>
        </xsl:variable>
        <xsl:element name="dimension">
          <xsl:attribute name="ref"
                         select="concat($gridspan/mine:MinCol, $gridspan/mine:MinRow, ':', $gridspan/mine:MaxCol, $gridspan/mine:MaxRow)"/>
        </xsl:element>
        <sheetViews>
          <sheetView workbookViewId="0" />
        </sheetViews>
        <sheetFormatPr defaultRowHeight="15" x14ac:dyDescent="0.25"/>
        <cols>
          <xsl:if test="$hasToken">
            <col min="11" max="11" width="16" customWidth="1" />
            <col min="12" max="12" width="12.7109375" customWidth="1"/>
            <col min="13" max="14" width="15.7109375" customWidth="1"/>
          </xsl:if>
          <xsl:if test="not($hasToken)">
            <col min="11" max="11" width="12.7109375" customWidth="1"/>
            <col min="12" max="13" width="15.7109375" customWidth="1"/>
          </xsl:if>
        </cols>
        <xsl:element name="sheetData">
          <row r="5" spans="11:18" x14ac:dyDescent="0.25">
            <xsl:if test="$hasToken">
              <xsl:element name="c">
                <xsl:attribute name="r" select="concat(mine:numToCol($tableOffset/mine:x), $tableOffset/mine:y)" />
                <xsl:attribute name="s" select="'1'" />
                <xsl:attribute name="t" select="'s'" />
                <xsl:element name="v">
                  <xsl:value-of select="mine:getSSNdx($rd//TokenName)" />
                </xsl:element>
              </xsl:element>
            </xsl:if>
            <xsl:variable name="startCol" select="xs:integer($tableOffset/mine:x) + (if ($hasToken) then 1 else 0)" />
            <xsl:element name="c">
              <xsl:attribute name="r" select="concat(mine:numToCol($startCol), $tableOffset/mine:y)" />
              <xsl:attribute name="s" select="'1'" />
              <xsl:attribute name="t" select="'s'" />
              <xsl:element name="v">
                <xsl:value-of select="mine:getSSNdx('Result Set')" />
              </xsl:element>
            </xsl:element>
            <xsl:element name="c">
              <xsl:attribute name="r" select="concat(mine:numToCol($startCol + 1), $tableOffset/mine:y)" />
              <xsl:attribute name="s" select="'1'" />
              <xsl:attribute name="t" select="'s'" />
              <xsl:element name="v">
                <xsl:value-of select="mine:getSSNdx('IAT Score')" />
              </xsl:element>
            </xsl:element>
            <xsl:element name="c">
              <xsl:attribute name="r" select="concat(mine:numToCol($startCol + 2), $tableOffset/mine:y)" />
              <xsl:attribute name="s" select="'1'" />
              <xsl:attribute name="t" select="'s'" />
              <xsl:element name="v">
                <xsl:value-of select="mine:getSSNdx('Error Count')" />
              </xsl:element>
            </xsl:element>
            <xsl:variable name="sAttr" select="if (count($rd//SurveyDesign//SurveyFormat) gt 0) then 3 else 2" />
            <xsl:element name="c">
              <xsl:attribute name="r" select="concat(mine:numToCol($startCol + 3), $tableOffset/mine:y)" />
              <xsl:attribute name="s" select="$sAttr" />
              <xsl:attribute name="t" select="'s'" />
              <xsl:element name="v">
                <xsl:value-of select="mine:getSSNdx('Latencies')" />
              </xsl:element>
            </xsl:element>
            <xsl:for-each select="for $i in ($startCol + 4) to ($startCol + 8) return $i">
              <xsl:element name="c">
                <xsl:attribute name="r" select="concat(mine:numToCol(.), $tableOffset/mine:y)" />
                <xsl:attribute name="s" select="$sAttr" />
              </xsl:element>
            </xsl:for-each>
          </row>
          <xsl:for-each select="$rd//TestResult[some $n in IATResult/IATResponse/ItemNum satisfies xs:integer($n) eq xs:integer($ItemNum)]">
            <xsl:call-template name="OutputTesteeRow">
              <xsl:with-param name="TestResult" select="."/>
              <xsl:with-param name="TesteeNum" select="count(preceding::TestResult) + 1"/>
              <xsl:with-param name="RowNum" select="position() + xs:integer($tableOffset/mine:y)"/>
              <xsl:with-param name="StartCol" select="xs:integer($tableOffset/mine:x)"/>
              <xsl:with-param name="ItemNum" select="$ItemNum"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:element>
        <mergeCells count="1">
          <xsl:element name="mergeCell">
            <xsl:attribute name="ref" select="concat(mine:numToCol($tableOffset/mine:x + 3 + (if ($hasToken) then 1 else 0)), $tableOffset/mine:y, ':', mine:numToCol($tableOffset/mine:x + 7 + (if ($hasToken) then 1 else 0)), $tableOffset/mine:y)" />
          </xsl:element>
        </mergeCells>
        <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
        <drawing r:id="rId1" />
      </xsl:element>
  </xsl:template>

  <xsl:template name="OutputTesteeRow">
    <xsl:param name="TestResult"/>
    <xsl:param name="TesteeNum"/>
    <xsl:param name="RowNum"/>
    <xsl:param name="StartCol"/>
    <xsl:param name="ItemNum"/>
    <xsl:element name="row">
      <xsl:attribute name="r" select="$RowNum"/>
      <xsl:attribute name="spans"
                     select="concat($StartCol, ':', xs:integer($StartCol) + max((8, count($TestResult/IATResult/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)]) + 2)) - (if ($hasToken) then 0 else 1))"/>
      <xsl:attribute name="x14ac:dyDescent" select="0.25"/>
      <xsl:if test="$hasToken">
        <xsl:element name="c">
          <xsl:attribute name="r" select="concat(mine:numToCol(xs:integer($StartCol)), $RowNum)" />
          <xsl:attribute name="s" select="'3'" />
          <xsl:attribute name="t" select="'s'" />
          <xsl:element name="v">
            <xsl:value-of select="mine:getSSNdx($TestResult/Token)" />
          </xsl:element>
        </xsl:element>
      </xsl:if>
      <xsl:variable name="offsetCol" select="xs:integer(xs:integer($StartCol) + (if ($hasToken) then 1 else 0))" />
      <xsl:element name="c">
        <xsl:attribute name="r" select="concat(mine:numToCol(xs:integer($offsetCol)), $RowNum)"/>
        <xsl:element name="v">
          <xsl:value-of select="$TesteeNum"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="c">
        <xsl:attribute name="r"
                       select="concat(mine:numToCol(xs:integer($offsetCol) + 1), $RowNum)"/>
        <xsl:choose>
          <xsl:when test="$TestResult/IATResult/IATScore eq 'NaN'">
            <xsl:attribute name="s" select="'1'" />
            <xsl:attribute name="t" select="'s'" />
            <xsl:element name="v">
              <xsl:value-of select="mine:getSSNdx('Unscored')" />
            </xsl:element>
          </xsl:when>
          <xsl:otherwise>
            <xsl:element name="v">
              <xsl:value-of select="$TestResult/IATResult/IATScore"/>
            </xsl:element>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <xsl:element name="c">
        <xsl:attribute name="r" select="concat(mine:numToCol(xs:integer($offsetCol) + 2), $RowNum)"/>
        <xsl:element name="v">
          <xsl:value-of select="count($TestResult/IATResult/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)][Error eq 'true'])" />
        </xsl:element>
      </xsl:element>
      <xsl:for-each select="$TestResult/IATResult/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)]">
        <xsl:element name="c">
          <xsl:attribute name="r"
                         select="concat(mine:numToCol(xs:integer($offsetCol) + position() + 2), $RowNum)"/>
          <xsl:element name="v">
            <xsl:value-of select="Latency"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:function name="mine:getSSNdx">
    <xsl:param name="str" />
    <xsl:value-of select="xs:integer($rd//Addendum/SharedStrings/SharedString[.=$str]/@Index) - 1" />
  </xsl:function>

</xsl:stylesheet>