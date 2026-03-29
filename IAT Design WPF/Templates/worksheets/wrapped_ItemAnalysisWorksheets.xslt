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

  <xsl:variable name="tableOffset">
    <mine:x>11</mine:x>
    <mine:y>5</mine:y>
  </xsl:variable>

  <xsl:template match="/ResultDocument">
    <xsl:element name="XMLWrapper" namespace="">
      <xsl:call-template name="OutputTitleWorksheet" />
      <xsl:call-template name="OutputSummaryWorksheet" />
      <xsl:call-template name="OutputLatencyWorksheet" />
      <xsl:for-each select="distinct-values(//IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending"/>
        <xsl:call-template name="OutputItemAnalSheet">
          <xsl:with-param name="ItemNum" select="." />
          <xsl:with-param name="SheetNum" select="position() + 3" />
        </xsl:call-template>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputTitleWorksheet">
    <xsl:element name="WrappedElement" namespace="">
      <xsl:attribute name="Path" select="'/xl/worksheets/sheet1.xml'" />
      <xsl:element name="worksheet">
        <xsl:namespace name="r"
                       select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'"/>
        <xsl:namespace name="mc"
                       select="'http://schemas.openxmlformats.org/markup-compatibility/2006'"/>
        <xsl:namespace name="x14ac"
                       select="'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'"/>
        <xsl:attribute name="mc:Ignorable" select="'x14ac'" />
        <dimension ref="A1"/>
        <sheetViews>
          <sheetView workbookViewId="0" />
        </sheetViews>
        <sheetFormatPr defaultRowHeight="15" x14ac:dyDescent="0.25"/>
        <sheetData/>
        <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
        <drawing r:id="rId1"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputSummaryWorksheet">
    <xsl:element name="WrappedElement" namespace="">
      <xsl:attribute name="Path" select="'/xl/worksheets/sheet2.xml'" />
      <xsl:element name="worksheet">
        <xsl:namespace name="r" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'" />
        <xsl:namespace name="mc" select="'http://schemas.openxmlformats.org/markup-compatibility/2006'" />
        <xsl:namespace name="x14ac" select="'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'" />
        <xsl:attribute name="mc:Ignorable" select="'x14ac'" />
        <xsl:variable name="dataDim">
          <mine:width>
            <xsl:value-of select="1 + count(SurveyDesign/SurveyFormat/Questions[@ResponseType ne 'None'])" />
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
              <xsl:apply-templates select="SurveyResults[xs:integer(@ElementNum) lt xs:integer($testResultNode/IATResult/TestElement)]/Answer">
                <xsl:with-param name="rowNum" select="$resultNum" />
                <xsl:with-param name="testResult" select="." />
              </xsl:apply-templates>
              <xsl:apply-templates select="IATResult">
                <xsl:with-param name="rowNum" select="$resultNum" />
              </xsl:apply-templates>
              <xsl:apply-templates select="SurveyResults[xs:integer(@ElementNum) gt xs:integer($testResultNode/IATResult/TestElement)]/Answer">
                <xsl:with-param name="rowNum" select="$resultNum" />
                <xsl:with-param name="testResult" select="." />
              </xsl:apply-templates>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
        <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Answer">
    <xsl:param name="rowNum" />
    <xsl:param name="testResult" />
    <xsl:variable name="answer" select="." />
    <xsl:variable name="colNum" select="count($testResult/child::SurveyResults[xs:integer(@ElementNum) lt xs:integer($answer/parent::SurveyResults/@ElementNum)]/Answer) + count($answer/preceding-sibling::Answer) + 1 + (if (xs:integer(parent::SurveyResults/@ElementNum) gt (count(distinct-values(parent::SurveyResults/preceding-sibling::SurveyResults)) + 1)) then 1 else 0)" />
    <xsl:variable name="testElemNum" select="xs:integer(parent::SurveyResults/@ElementNum)" />
    <xsl:element name="c">
      <xsl:variable name="col" select="mine:numToCol($colNum)" />
      <xsl:attribute name="r" select="concat($col, $rowNum)" />
      <xsl:variable name="respType" select="//SurveyFormat[xs:integer(@ElementNum) eq $testElemNum]/Questions[@ResponseType ne 'None'][count($answer/preceding-sibling::Answer) + 1]/@ResponseType" />
      <xsl:variable name="numRespTypes" select="tokenize('Likert WeightedMultiple BoundedNum Multiple', ' ')" />
      <xsl:variable name="sharedStrRespTypes" select="tokenize('MultiBoolean Date RegEx BoundedLength Boolean FixedDig', ' ')" />
      <xsl:choose>
        <xsl:when test="count(distinct-values($numRespTypes[.=$respType])) gt 0">
          <xsl:attribute name="s" select="'2'" />
          <xsl:element name="v">
            <xsl:value-of select="." />
          </xsl:element>
        </xsl:when>
        <xsl:when test="count(distinct-values($sharedStrRespTypes[.=$respType])) gt 0">
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
  </xsl:template>

  <xsl:template match="IATResult">
    <xsl:param name="rowNum" />
    <xsl:variable name="ir" select="." />
    <xsl:variable name="colNum" select="count(parent::TestResult/descendant::Answer[xs:integer(parent::SurveyResults/@ElementNum) lt xs:integer($ir/TestElement)]) + 1" />
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
          <xsl:value-of select="IATScore" />
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputLatencyWorksheet">
    <xsl:element name="WrappedElement" namespace="">
      <xsl:attribute name="Path" select="'/xl/worksheets/sheet3.xml'" />
      <xsl:element name="worksheet">
        <xsl:namespace name="r" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'" />
        <xsl:namespace name="mc" select="'http://schemas.openxmlformats.org/markup-compatibility/2006'" />
        <xsl:namespace name="x14ac" select="'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'" />
        <xsl:attribute name="mc:Ignorable" select="'x14ac'" />
        <xsl:variable name="dataDim">
          <mine:width>
            <xsl:value-of select="1 + 2 * count(//TestResult)" />
          </mine:width>
          <mine:height>
            <xsl:value-of select="2 + count(//NumBlockPresentations) + sum(//NumBlockPresentations)" />
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
          <xsl:if test="concat($gridspan/mine:MinCol, $gridspan/MinRow) ne concat($gridspan/mine:MaxCol, $gridspan/mine:MaxRow)">
            <xsl:attribute name="ref" select="concat($gridspan/mine:MinCol, $gridspan/mine:MinRow, ':', $gridspan/mine:MaxCol, $gridspan/mine:MaxRow)" />
          </xsl:if>
          <xsl:if test="concat($gridspan/mine:MinCol, $gridspan/MinRow) eq concat($gridspan/mine:MaxCol, $gridspan/mine:MaxRow)">
            <xsl:attribute name="ref" select="concat($gridspan/mine:MinCol, $gridspan/mine:MinRow)" />
          </xsl:if>
        </xsl:element>
        <sheetViews>
          <sheetView workbookViewId="0" />
        </sheetViews>
        <sheetFormatPr defaultRowHeight="15" x14ac:dyDescent="0.25"/>
        <cols>
          <xsl:for-each select="for $i in 1 to count(//TestResult) return (2 * $i + 1)">
            <xsl:element name="col">
              <xsl:attribute name="min" select="." />
              <xsl:attribute name="max" select="." />
              <xsl:attribute name="width" select="15" />
              <xsl:attribute name="customWidth" select="1" />
            </xsl:element>
          </xsl:for-each>
        </cols>
        <xsl:element name="sheetData">
          <xsl:element name="row">
            <xsl:attribute name="r" select="'1'" />
            <xsl:attribute name="spans" select="concat('1:', 1 + 2 * count(//TestResult))" />
            <xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
            <xsl:for-each select="for $i in 1 to 2 * count(//TestResult) return $i + 1">
              <xsl:element name="c">
                <xsl:attribute name="r" select="concat(mine:numToCol(.), '1')" />
                <xsl:attribute name="s" select="'1'" />
                <xsl:attribute name="t" select="'s'" />
                <xsl:element name="v">
                  <xsl:if test=". mod 2 eq 0">
                    <xsl:value-of select="mine:getSSNdx('Item #')" />
                  </xsl:if>
                  <xsl:if test=". mod 2 eq 1">
                    <xsl:value-of select="mine:getSSNdx('Latency (ms)')" />
                  </xsl:if>
                </xsl:element>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>

          <xsl:variable name="rows">
            <xsl:for-each select="//NumBlockPresentations">
              <xsl:variable name="blockNum" select="position()" />
              <xsl:variable name="presEntry" select="." />
              <xsl:for-each select="1 to xs:integer(.)">
                <xsl:variable name="rowNum" select="sum($presEntry/preceding-sibling::NumBlockPresentations) + position() + count($presEntry/preceding-sibling::NumBlockPresentations)" />
                <xsl:variable name="dataRow" select="sum($presEntry/preceding-sibling::NumBlockPresentations) + position()" />
                <xsl:element name="row">
                  <xsl:call-template name="outputResultRow">
                    <xsl:with-param name="itemResults" select="$rd//TestResult/IATResult/IATResponse[position() eq $dataRow]" />
                    <xsl:with-param name="row" select="$rowNum + 1" />
                  </xsl:call-template>
                </xsl:element>
              </xsl:for-each>
            </xsl:for-each>
          </xsl:variable>

          <xsl:for-each select="//NumBlockPresentations">
            <xsl:variable name="startRow" select="sum(preceding-sibling::NumBlockPresentations) + count(preceding-sibling::NumBlockPresentations) + 2" />
            <xsl:variable name="startPres" select="sum(preceding-sibling::NumBlockPresentations) + 1" />
            <xsl:variable name="blockNum" select="position()" />
            <xsl:element name="row">
              <xsl:attribute name="r" select="$startRow" />
              <xsl:attribute name="spans" select="concat('1:', count($rd//TestResult) * 2 + 1)" />
              <xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
              <xsl:element name="c">
                <xsl:attribute name="r" select="concat('A', $startRow)" />
                <xsl:attribute name="s" select="'1'" />
                <xsl:attribute name="t" select="'s'" />
                <xsl:element name="v">
                  <xsl:value-of select="mine:getSSNdx(concat('Block ', position()))" />
                </xsl:element>
              </xsl:element>
              <xsl:copy-of select="$rows/child::*[xs:integer($startPres)]/child::*" />
            </xsl:element>
            <xsl:for-each select="1 to (xs:integer(.) - 1)">
              <xsl:variable name="rowNum" select="$startPres + xs:integer(.)" />
              <xsl:element name="row">
                <xsl:attribute name="r" select="$startRow + xs:integer(.)" />
                <xsl:attribute name="spans" select="concat('1:', count($rd//TestResult) * 2 + 1)" />
                <xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
                <xsl:sequence select="$rows/child::*[$rowNum]/child::*" />
              </xsl:element>
            </xsl:for-each>
          </xsl:for-each>

          <xsl:element name="row">
            <xsl:variable name="rowNum" select="count(//NumBlockPresentations) + sum(//NumBlockPresentations) + 2" />
            <xsl:attribute name="r" select="$rowNum" />
            <xsl:attribute name="spans" select="concat('1:', 1 + 2 * count(//TestResult))" />
            <xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
            <xsl:element name="c">
              <xsl:attribute name="r" select="concat('A', $rowNum)" />
              <xsl:attribute name="s" select="'1'" />
              <xsl:attribute name="t" select="'s'" />
              <xsl:element name="v">
                <xsl:value-of select="mine:getSSNdx('IAT Score')" />
              </xsl:element>
            </xsl:element>
            <xsl:for-each select="//TestResult">
              <xsl:element name="c">
                <xsl:attribute name="r" select="concat(mine:numToCol(position() * 2 + 1), $rowNum)" />
                <xsl:if test="IATResult/IATScore eq 'NaN'">
                  <xsl:attribute name="s" select="'1'" />
                  <xsl:attribute name="t" select="'s'" />
                  <xsl:element name="v">
                    <xsl:value-of select="mine:getSSNdx('Unscored')" />
                  </xsl:element>
                </xsl:if>
                <xsl:if test="IATResult/IATScore ne 'NaN'">
                  <xsl:attribute name="s" select="'2'" />
                  <xsl:element name="v">
                    <xsl:value-of select="IATResult/IATScore" />
                  </xsl:element>
                </xsl:if>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
        <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="outputResultRow">
    <xsl:param name="itemResults" />
    <xsl:param name="row" />
    <xsl:for-each select="$itemResults">
      <xsl:element name="c">
        <xsl:attribute name="r" select="concat(mine:numToCol(position() * 2), $row)" />
        <xsl:element name="v">
          <xsl:value-of select="ItemNum" />
        </xsl:element>
      </xsl:element>
      <xsl:element name="c">
        <xsl:attribute name="r" select="concat(mine:numToCol(position() * 2 + 1), $row)" />
        <xsl:element name="v">
          <xsl:value-of select="Latency" />
        </xsl:element>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>


  <xsl:template name="OutputItemAnalSheet">
    <xsl:param name="ItemNum" />
    <xsl:param name="SheetNum" />
    <xsl:element name="WrappedElement" namespace="">
      <xsl:attribute name="Path" select="concat('/xl/worksheets/sheet', $SheetNum, '.xml')" />
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
            <xsl:value-of select="max((8, for $n in $rd//TestResult[some $in in IATResult/IATResponse/ItemNum satisfies xs:integer($in) eq xs:integer($ItemNum)] return count($n/IATResult/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)]) + 2))"/>
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
          <col min="11" max="11" width="12.7109375" customWidth="1"/>
          <col min="12" max="13" width="15.7109375" customWidth="1"/>
        </cols>
        <xsl:element name="sheetData">
          <row r="5" spans="11:18" x14ac:dyDescent="0.25">
            <c r="K5" s="1" t="s">
              <v>
                <xsl:value-of select="mine:getSSNdx('Result Set')" />
              </v>
            </c>
            <c r="L5" s="1" t="s">
              <v>
                <xsl:value-of select="mine:getSSNdx('IAT Score')" />
              </v>
            </c>
            <c r="M5" s="1" t="s">
              <v>
                <xsl:value-of select="mine:getSSNdx('Error Count')" />
              </v>
            </c>
            <xsl:variable name="sAttr" select="if (count($rd//SurveyDesign//SurveyFormat) gt 0) then 3 else 2" />
            <xsl:element name="c">
              <xsl:attribute name="r" select="'N5'" />
              <xsl:attribute name="s" select="$sAttr" />
              <xsl:attribute name="t" select="'s'" />
              <xsl:element name="v">
                <xsl:value-of select="mine:getSSNdx('Latencies')" />
              </xsl:element>
            </xsl:element>
            <xsl:element name="c">
              <xsl:attribute name="r" select="'O5'" />
              <xsl:attribute name="s" select="$sAttr" />
            </xsl:element>
            <xsl:element name="c">
              <xsl:attribute name="r" select="'P5'" />
              <xsl:attribute name="s" select="$sAttr" />
            </xsl:element>
            <xsl:element name="c">
              <xsl:attribute name="r" select="'Q5'" />
              <xsl:attribute name="s" select="$sAttr" />
            </xsl:element>
            <xsl:element name="c">
              <xsl:attribute name="r" select="'R5'" />
              <xsl:attribute name="s" select="$sAttr" />
            </xsl:element>
          </row>
          <xsl:for-each select="$rd//TestResult[some $n in IATResult/IATResponse/ItemNum satisfies xs:integer($n) eq xs:integer($ItemNum)]">
            <xsl:call-template name="OutputTesteeRow">
              <xsl:with-param name="IATResultSet" select="IATResult"/>
              <xsl:with-param name="TesteeNum" select="count(preceding::TestResult) + 1"/>
              <xsl:with-param name="RowNum" select="position() + xs:integer($tableOffset/mine:y)"/>
              <xsl:with-param name="StartCol" select="xs:integer($tableOffset/mine:x)"/>
              <xsl:with-param name="ItemNum" select="$ItemNum"/>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:element>
        <mergeCells count="1">
          <mergeCell ref="N5:R5"/>
        </mergeCells>
        <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
        <drawing r:id="rId1" />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputTesteeRow">
    <xsl:param name="IATResultSet"/>
    <xsl:param name="TesteeNum"/>
    <xsl:param name="RowNum"/>
    <xsl:param name="StartCol"/>
    <xsl:param name="ItemNum"/>
    <xsl:element name="row">
      <xsl:attribute name="r" select="$RowNum"/>
      <xsl:attribute name="spans"
                     select="concat($StartCol, ':', xs:integer($StartCol) + max((8, count($IATResultSet/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)]) + 2)) - 1)"/>
      <xsl:attribute name="x14ac:dyDescent" select="0.25"/>
      <xsl:element name="c">
        <xsl:attribute name="r" select="concat(mine:numToCol(xs:integer($StartCol)), $RowNum)"/>
        <xsl:element name="v">
          <xsl:value-of select="$TesteeNum"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="c">
        <xsl:attribute name="r"
                       select="concat(mine:numToCol(xs:integer($StartCol) + 1), $RowNum)"/>
        <xsl:choose>
          <xsl:when test="$IATResultSet/IATScore eq 'NaN'">
            <xsl:attribute name="s" select="'1'" />
            <xsl:attribute name="t" select="'s'" />
            <xsl:element name="v">
              <xsl:value-of select="mine:getSSNdx('Unscored')" />
            </xsl:element>
          </xsl:when>
          <xsl:otherwise>
            <xsl:element name="v">
              <xsl:value-of select="$IATResultSet/IATScore"/>
            </xsl:element>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <xsl:element name="c">
        <xsl:attribute name="r" select="concat(mine:numToCol(xs:integer($StartCol) + 2), $RowNum)"/>
        <xsl:element name="v">
          <xsl:value-of select="count($IATResultSet/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)][Error eq 'true'])" />
        </xsl:element>
      </xsl:element>
      <xsl:for-each select="$IATResultSet/IATResponse[xs:integer(ItemNum) eq xs:integer($ItemNum)]">
        <xsl:element name="c">
          <xsl:attribute name="r"
                         select="concat(mine:numToCol(xs:integer($StartCol) + position() + 2), $RowNum)"/>
          <xsl:element name="v">
            <xsl:value-of select="Latency"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:function name="mine:getSSNdx">
    <xsl:param name="str" />
    <xsl:variable name="staticStrings">
      <mine:heading>Result Set</mine:heading>
      <mine:heading>IAT Score</mine:heading>
      <mine:heading>Error Count</mine:heading>
      <mine:heading>Latencies</mine:heading>
      <mine:heading>Unscored</mine:heading>
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
      <xsl:for-each select="for $i in 1 to count($rd//SurveyDesign//Questions[@ResponseType ne 'None']) return $i">
        <xsl:variable name="questNum" select="xs:integer(.)" />
        <xsl:if test="$rd//SurveyDesign//Questions[@ResponseType ne 'None'][$questNum]/@ResponseType[.=$strRespTypes/child::*]">
          <xsl:for-each select="$rd//TestResult/SurveyResults/Answer[position() eq xs:integer($questNum)]">
            <xsl:element name="mine:string">
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:for-each>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="distinctStrings">
      <xsl:for-each select="distinct-values($Strings/mine:string)">
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
    <xsl:value-of select="index-of($distinctStrings/mine:string, $str) - 1" />
  </xsl:function>
</xsl:stylesheet>