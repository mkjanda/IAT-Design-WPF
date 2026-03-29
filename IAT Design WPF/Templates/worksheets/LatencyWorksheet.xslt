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

	<xsl:variable name="hasToken" select="count(//TestResult/Token) gt 0" />

	<xsl:template match="/">
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
					<xsl:value-of select="2 + count(//NumBlockPresentations) + sum(//NumBlockPresentations) + (if ($hasToken) then 1 else 0)" />
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
						<xsl:attribute name="width" select="16" />
						<xsl:attribute name="customWidth" select="1" />
					</xsl:element>
				</xsl:for-each>
			</cols>
			<xsl:element name="sheetData">

				<xsl:if test="$hasToken">
					<xsl:element name="row">
						<xsl:attribute name="r" select="'1'" />
						<xsl:attribute name="spans" select="concat('1:', 1 + 2 * count(//TestResult))" />
						<xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
						<xsl:element name="c">
							<xsl:attribute name="r" select="'A1'" />
							<xsl:attribute name="s" select="'2'" />
							<xsl:attribute name="t" select="'s'" />
							<xsl:element name="v">
								<xsl:value-of select="mine:getSSNdx(//TokenName)" />
							</xsl:element>
						</xsl:element>
						<xsl:for-each select="for $i in 1 to count(//TestResult) return $i">
							<xsl:variable name="ndx" select="." />
							<xsl:element name="c">
								<xsl:attribute name="r" select="concat(mine:numToCol(2 * xs:integer($ndx)), '1')" />
								<xsl:attribute name="s" select="'3'" />
								<xsl:attribute name="t" select="'s'" />
								<xsl:element name="v">
									<xsl:value-of select="mine:getSSNdx($rd//TestResult[xs:integer($ndx)]/Token)" />
								</xsl:element>
							</xsl:element>
							<xsl:element name="c">
								<xsl:attribute name="r" select="concat(mine:numToCol(2 * xs:integer(.) + 1), '1')" />
								<xsl:attribute name="s" select="'3'" />
							</xsl:element>
						</xsl:for-each>
					</xsl:element>
				</xsl:if>



				<xsl:element name="row">
					<xsl:attribute name="r" select="if ($hasToken) then '2' else '1'" />
					<xsl:attribute name="spans" select="concat('1:', 1 + 2 * count(//TestResult))" />
					<xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
					<xsl:for-each select="for $i in 1 to 2 * count(//TestResult) return $i + 1">
						<xsl:element name="c">
							<xsl:attribute name="r" select="concat(mine:numToCol(.), if ($hasToken) then '2' else '1')" />
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
							<xsl:variable name="rowNum" select="sum($presEntry/preceding-sibling::NumBlockPresentations) + position() + count($presEntry/preceding-sibling::NumBlockPresentations) + (if ($hasToken) then 1 else 0)" />
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
					<xsl:variable name="startRow" select="sum(preceding-sibling::NumBlockPresentations) + count(preceding-sibling::NumBlockPresentations) + 2 + (if ($hasToken) then 1 else 0)" />
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
					<xsl:variable name="rowNum" select="count(//NumBlockPresentations) + sum(//NumBlockPresentations) + 2 + (if ($hasToken) then 1 else 0)" />
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
			<xsl:if test="$hasToken">
			<xsl:element name="mergeCells">
				<xsl:for-each select="for $i in 1 to count($rd//TestResult) return ($i * 2 + 1)">
						<xsl:element name="mergeCell">
							<xsl:attribute name="ref" select="concat(mine:numToCol(. - 1), '1:', mine:numToCol(.), '1')" />
						</xsl:element>
					</xsl:for-each>
				</xsl:element>
			</xsl:if>
			<pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
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

	<xsl:function name="mine:getSSNdx">
		<xsl:param name="str" />
		<xsl:value-of select="xs:integer($rd//Addendum/SharedStrings/SharedString[.=$str]/@Index) - 1" />
	</xsl:function>

</xsl:stylesheet>