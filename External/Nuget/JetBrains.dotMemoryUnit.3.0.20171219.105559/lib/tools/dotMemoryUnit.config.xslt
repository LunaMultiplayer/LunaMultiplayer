<?xml version='1.0'?>
<xsl:stylesheet version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dmu="DotMemoryUnit">
  
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/dmu:DotMemoryUnitTestRuntimeInfo">
    <DotMemoryUnitServerStartInfo xmlns="DotMemoryUnit">
      <Executable>dotMemoryUnit.exe</Executable>
    </DotMemoryUnitServerStartInfo>
  </xsl:template>
  
</xsl:stylesheet>